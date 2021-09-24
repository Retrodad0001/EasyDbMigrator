using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public class DbMigrator
    {
        private readonly ILogger _logger;
        private readonly MigrationConfiguration _migrationConfiguration;
        private readonly IDatabaseConnector _databaseconnector;
        private readonly IAssemblyResourceHelper _assemblyResourceHelper;

        public DbMigrator(ILogger logger
            , MigrationConfiguration migrationConfiguration
            , IDatabaseConnector databaseconnector
            , IAssemblyResourceHelper assemblyResourceHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _migrationConfiguration = migrationConfiguration ?? throw new ArgumentNullException(nameof(migrationConfiguration));
            _databaseconnector = databaseconnector ?? throw new ArgumentNullException(nameof(databaseconnector));
            _assemblyResourceHelper = assemblyResourceHelper ?? throw new ArgumentNullException(nameof(assemblyResourceHelper));
        }

        public async Task<bool> TryApplyMigrationsAsync(Type customClass, DateTime executedDateTime)
        {
            _logger.Log(logLevel: LogLevel.Information, message: $"start running migrations for database: {_migrationConfiguration.DatabaseName}");
            
            Result<bool> setupDatabaseSucceeded;
            Result<bool> createVersiongTableSucceeded = new(isSucces: false); 
            bool migrationRunwithoutUnknownExceptions = false;

            setupDatabaseSucceeded = await TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(migrationConfiguration: _migrationConfiguration);

            if (setupDatabaseSucceeded.IsFailure)
                _logger.Log(logLevel: LogLevel.Error, exception: setupDatabaseSucceeded.Exception, @"setup database when there is none with default settings: error occurred");
            else
                _logger.Log(logLevel: LogLevel.Information, message: @"setup database when there is none with default settings executed successfully");

            if (setupDatabaseSucceeded.IsSuccess)
            {
                createVersiongTableSucceeded = await TrySetupDbMigrationsRunTableWhenNotExcistAsync(migrationConfiguration: _migrationConfiguration);

                if (createVersiongTableSucceeded.IsFailure)
                    _logger.Log(logLevel: LogLevel.Error, exception: createVersiongTableSucceeded.Exception, @"setup DbMigrationsRun when there is none executed with errors");
                else
                    _logger.Log(logLevel:LogLevel.Information, message: @"setup DbMigrationsRun when there is none executed successfully");

                if (createVersiongTableSucceeded.IsSuccess)
                {
                    migrationRunwithoutUnknownExceptions = await TryRunAllMigrationScriptsAsync(migrationConfiguration: _migrationConfiguration
                        , customclassType: customClass
                        , executedDateTime: executedDateTime);
                }
            }

            if (setupDatabaseSucceeded.IsFailure || createVersiongTableSucceeded.IsFailure || !migrationRunwithoutUnknownExceptions)
            {
                _logger.Log(logLevel: LogLevel.Error, exception:null , message: "Whole migration process executed with errors");
                return false;
            }

            _logger.Log(logLevel: LogLevel.Information, message: "Whole migration process executed successfully");

            return true;
        }

        private async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration)
        {
            string sqlScriptCreateDatabase = @$" 
                USE Master
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{migrationConfiguration.DatabaseName}')
                BEGIN
                    CREATE DATABASE {migrationConfiguration.DatabaseName}
                END";

            Result<bool> result = await _databaseconnector.TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                , scriptName: "EasyDbMigrator.SetupEmptyDb"
                , sqlScriptContent: sqlScriptCreateDatabase);

            return result;
        }

        private async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(MigrationConfiguration migrationConfiguration)
        {
            string sqlScriptCreateMigrationTable = @$" USE {migrationConfiguration.DatabaseName}  
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DbMigrationsRun' AND xtype='U')
                BEGIN
                    CREATE TABLE DbMigrationsRun 
                    (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Executed Datetime2 NOT NULL,
                        Filename nvarchar(100) NOT NULL UNIQUE,
                        Version nvarchar(10) NOT NULL
                    )
                END";

            Result<bool> result = await _databaseconnector.TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                , scriptName: "EasyDbMigrator.SetupDbMigrationsRunTable"
                , sqlScriptContent: sqlScriptCreateMigrationTable);

            return result;
        }

        private async Task<bool> TryRunAllMigrationScriptsAsync(MigrationConfiguration migrationConfiguration
            , Type customclassType
            , DateTime executedDateTime)
        {
            List<Script> unOrderedScripts = await _assemblyResourceHelper.TryConverManifestResourceStreamsToScriptsAsync(customclassType);
            List<Script> orderedScripts = SetScriptsInCorrectSequence(unOrderedScripts);

            _logger.Log(logLevel: LogLevel.Information, message: $"Total scripts found: {orderedScripts.Count}");

            bool skipBecauseOfErrorWithPreviousScript = false;
            foreach (Script script in orderedScripts)
            {
                if (skipBecauseOfErrorWithPreviousScript)
                {
                    _logger.Log(logLevel: LogLevel.Warning, message: $"script: {script.FileName} was skipped due to exception in previous script");
                    continue;
                }

                Result<RunMigrationResult> result = await _databaseconnector.RunDbMigrationScriptWhenNotRunnedBeforeAsync(migrationConfiguration: migrationConfiguration
                    , script: script
                    , executedDateTime: executedDateTime);

                switch (result.Value)
                {
                    case RunMigrationResult.MigrationScriptExecuted:
                        _logger.Log(logLevel: LogLevel.Information, message: $"script: {script.FileName} was run");
                        break;
                    case RunMigrationResult.ScriptSkippedBecauseAlreadyRun:
                        _logger.Log(logLevel: LogLevel.Information, message: $"script: {script.FileName} was not run because script was already executed");
                        break;
                    case RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted:
                        _logger.Log(logLevel: LogLevel.Error, exception: result.Exception, message: $"script: {script.FileName} was not completed due to exception");
                        skipBecauseOfErrorWithPreviousScript = true;
                        break;
                    default:
                        break;
                }
            }

            if (skipBecauseOfErrorWithPreviousScript)
                return false;
            else
                return true;
        }

        private List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
        {
            return scripts.OrderBy(s => s.DatePartOfName)
                .ThenBy(s => s.SequenceNumberPart).ToList();
        }
    }
}
