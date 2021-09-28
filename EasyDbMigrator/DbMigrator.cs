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
        private readonly IDataTimeHelper _dataTimeHelper;
        private readonly List<string> _excludedScriptList = new List<string>();

        public DbMigrator(ILogger logger
            , MigrationConfiguration migrationConfiguration
            , IDatabaseConnector databaseconnector
            , IAssemblyResourceHelper assemblyResourceHelper
            , IDataTimeHelper dataTimeHelper)
        {

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));
            _logger = logger;

            if (migrationConfiguration is null)
                throw new ArgumentNullException(nameof(migrationConfiguration));
            _migrationConfiguration = migrationConfiguration;

            if (databaseconnector is null)
                throw new ArgumentNullException(nameof(databaseconnector));
            _databaseconnector = databaseconnector;

            if (assemblyResourceHelper is null)
                throw new ArgumentNullException(nameof(assemblyResourceHelper));
            _assemblyResourceHelper = assemblyResourceHelper;

            if (dataTimeHelper is null)
                throw new ArgumentNullException(nameof(dataTimeHelper));
            _dataTimeHelper = dataTimeHelper;
        }

        public static DbMigrator Create(MigrationConfiguration migrationConfiguration, ILogger logger)
        {
            if (migrationConfiguration is null)
            {
                throw new ArgumentNullException(nameof(migrationConfiguration));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            DbMigrator result = new DbMigrator(logger: logger
                , migrationConfiguration: migrationConfiguration
                , databaseconnector: new SqlDBConnector()
                , assemblyResourceHelper: new AssemblyResourceHelper()
                , dataTimeHelper: new DataTimeHelper());

            return result;
        }

        public static DbMigrator CreateForLocalIntegrationTesting(MigrationConfiguration migrationConfiguration
            , ILogger logger
            , IDataTimeHelper dataTimeHelperMock)
        {
            if (migrationConfiguration is null)
            {
                throw new ArgumentNullException(nameof(migrationConfiguration));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (dataTimeHelperMock is null)
            {
                throw new ArgumentNullException(nameof(dataTimeHelperMock));
            }

            DbMigrator result = new DbMigrator(logger: logger
               , migrationConfiguration: migrationConfiguration
               , databaseconnector: new SqlDBConnector()
               , assemblyResourceHelper: new AssemblyResourceHelper()
               , dataTimeHelper: dataTimeHelperMock);

            return result;
        }

        public async Task<bool> TryDeleteDatabaseIfExistAsync(string databaseName, string connectionString)
        {
            string query = $@"
                IF EXISTS(SELECT * FROM master.sys.databases WHERE name='{databaseName}')
                BEGIN               
                    ALTER DATABASE {databaseName} 
                    SET OFFLINE WITH ROLLBACK IMMEDIATE;
                    ALTER DATABASE {databaseName} SET ONLINE;
                    DROP DATABASE {databaseName};
                END
                ";

           Result<bool> succeeded = await _databaseconnector.TryExcecuteSingleScriptAsync(connectionString: connectionString
                , scriptName: "EasyDbMigrator.Integrationtest_dropDatabase"
                , sqlScriptContent: query);

            if (succeeded.IsFailure)
            {
                _logger.Log(logLevel: LogLevel.Error, exception: succeeded.Exception, "DeleteDatabaseIfExistAsync executed with error");
                return false;
            }

            return true;
        }

        public async Task<bool> TryApplyMigrationsAsync(Type customClass)
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
                    List<SqlScript> unOrderedScripts = await _assemblyResourceHelper.TryConverManifestResourceStreamsToScriptsAsync(customclass: customClass);
                    List<SqlScript> unOrderedScriptsWithoutExludedScripts = RemoveExcludedScripts(scripts: unOrderedScripts, excludedscripts: _excludedScriptList);
                    List<SqlScript> orderedScriptsWithoutExcludedScripts = SetScriptsInCorrectSequence(scripts: unOrderedScriptsWithoutExludedScripts);

                    _logger.Log(logLevel: LogLevel.Information, message: $"Total scripts found: {unOrderedScripts.Count}");

                    migrationRunwithoutUnknownExceptions = await TryRunAllMigrationScriptsAsync(migrationConfiguration: _migrationConfiguration
                        , orderedScripts: orderedScriptsWithoutExcludedScripts);
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

        public void ExcludeScripts(List<string> scriptsToExclude)
        {
            _excludedScriptList.AddRange(scriptsToExclude);
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
                , scriptName: "SetupEmptyDb"
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

        private async Task<bool> TryRunAllMigrationScriptsAsync(MigrationConfiguration migrationConfiguration, List<SqlScript> orderedScripts)
        {
            bool skipBecauseOfErrorWithPreviousScript = false;
            foreach (SqlScript script in orderedScripts)
            {
                if (skipBecauseOfErrorWithPreviousScript)
                {
                    _logger.Log(logLevel: LogLevel.Warning, message: $"script: {script.FileName} was skipped due to exception in previous script");
                    continue;
                }

                DateTime executedDateTime = _dataTimeHelper.GetCurrentUtcTime();

                Result<RunMigrationResult> result = await _databaseconnector.RunDbMigrationScriptWhenNotRunnedBeforeAsync(migrationConfiguration: migrationConfiguration
                    , script: script
                    , executedDateTime: executedDateTime);

                if (result.Value == RunMigrationResult.MigrationScriptExecuted)
                {
                    _logger.Log(logLevel: LogLevel.Information, message: $"script: {script.FileName} was run");
                }
                else if (result.Value == RunMigrationResult.ScriptSkippedBecauseAlreadyRun)
                {
                    _logger.Log(logLevel: LogLevel.Information, message: $"script: {script.FileName} was not run because script was already executed");
                }
                else if (result.Value == RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted)
                {
                    _logger.Log(logLevel: LogLevel.Error, exception: result.Exception, message: $"script: {script.FileName} was not completed due to exception");
                    skipBecauseOfErrorWithPreviousScript = true;
                }
            }

            if (skipBecauseOfErrorWithPreviousScript)
                return false;
            else
                return true;
        }

        private List<SqlScript> RemoveExcludedScripts(List<SqlScript> scripts, List<string> excludedscripts)
        {
            var result = scripts.Where(p => !excludedscripts.Any(x => x == p.FileName)).ToList();
            return result;
        }

        private List<SqlScript> SetScriptsInCorrectSequence(List<SqlScript> scripts)
        {
            return scripts.OrderBy(s => s.DatePartOfName)
                .ThenBy(s => s.SequenceNumberPart).ToList();
        }
    }
}
