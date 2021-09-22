using EasyDbMigrator.Helpers;
using EasyDbMigrator.Infra;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public class DbMigrator
    {
        private readonly ILogger _logger;
        private readonly ISqlDbHelper _sqlDbHelper;
        private readonly IScriptsHelper _scriptsHelper;

        public DbMigrator(ILogger logger, ISqlDbHelper sqlDbHelper, IScriptsHelper scriptsHelper)
        {
            if (sqlDbHelper is null)
                throw new ArgumentNullException(nameof(sqlDbHelper));
         
            if (scriptsHelper is null)
                throw new ArgumentNullException(nameof(scriptsHelper));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            _sqlDbHelper = sqlDbHelper;
            _scriptsHelper = scriptsHelper;
        }

        public async Task<bool> TryApplyMigrationsAsync(SqlDataBaseInfo sqlDataBaseInfo
            , Type customClass
            , DateTime executedDateTime)
        {
            if (sqlDataBaseInfo is null)
            {
                throw new ArgumentNullException(nameof(sqlDataBaseInfo));
            }

            Result<bool> setupDatabaseSucceeded;
            Result<bool> createVersiongTableSucceeded = new(isSucces: false); 
            bool migrationRunwithoutErrors = false;

            setupDatabaseSucceeded = await TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(sqlDataBaseInfo: sqlDataBaseInfo);

            if (setupDatabaseSucceeded.IsFailure)
                _logger.LogError(@"setup database when there is none with default settings: error occurred", setupDatabaseSucceeded.Exception);
            else
                _logger.LogInformation(@"setup database when there is none with default settings executed successfully");

            if (setupDatabaseSucceeded.IsSuccess)
            {
                createVersiongTableSucceeded = await TrySetupDbMigrationsRunTableWhenNotExcistAsync(sqlDataBaseInfo: sqlDataBaseInfo);

                if (createVersiongTableSucceeded.IsFailure)
                    _logger.LogError(@"setup DbMigrationsRun when there is none executed with errors", createVersiongTableSucceeded.Exception);
                else
                    _logger.LogInformation(@"setup DbMigrationsRun when there is none executed successfully");

                if (createVersiongTableSucceeded.IsSuccess)
                {
                    migrationRunwithoutErrors = await TryRunAllMigrationScriptsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                        , customclassType: customClass
                        , executedDateTime: executedDateTime);
                }
            }

            if (setupDatabaseSucceeded.IsFailure || createVersiongTableSucceeded.IsFailure || !migrationRunwithoutErrors)
            {
                _logger.LogError("Whole migration process executed with errors");
                return false;
            }

            _logger.LogInformation("Whole migration process executed successfully");

            return true;
        }

        private async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(SqlDataBaseInfo sqlDataBaseInfo)
        {
            string sqlScriptCreateDatabase = @$" 
                USE Master
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{sqlDataBaseInfo.DatabaseName}')
                BEGIN
                    CREATE DATABASE {sqlDataBaseInfo.DatabaseName}
                END";

            Result<bool> result = await _sqlDbHelper.TryExcecuteSingleScriptAsync(connectionString: sqlDataBaseInfo.ConnectionString
                , scriptName: "EasyDbMigrator.SetupEmptyDb"
                , sqlScriptContent: sqlScriptCreateDatabase);

            return result;
        }

        private async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(SqlDataBaseInfo sqlDataBaseInfo)
        {
            string sqlScriptCreateMigrationTable = @$" USE {sqlDataBaseInfo.DatabaseName}  
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

            Result<bool> result = await _sqlDbHelper.TryExcecuteSingleScriptAsync(connectionString: sqlDataBaseInfo.ConnectionString
                , scriptName: "EasyDbMigrator.SetupDbMigrationsRunTable"
                , sqlScriptContent: sqlScriptCreateMigrationTable);

            return result;
        }

        private async Task<bool> TryRunAllMigrationScriptsAsync(SqlDataBaseInfo sqlDataBaseInfo
            , Type customclassType
            , DateTime executedDateTime)
        {
            List<Script> orderedScripts = await _scriptsHelper.TryConvertoScriptsInCorrectSequenceByTypeAsync(customclassType);

            foreach (Script script in orderedScripts)
            {
                Result<RunMigrationResult> result = await _sqlDbHelper.RunDbMigrationScriptWhenNotRunnedBeforeAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , script: script
                    , executedDateTime: executedDateTime);

                if (result.IsSuccess)
                {
                    switch (result.Value)
                    {
                        case RunMigrationResult.MigrationScriptExecuted:
                            _logger.LogInformation($"script: {script.FileName} was run");
                            break;
                        case RunMigrationResult.IgnoredAllreadyRun:
                            _logger.LogInformation($"script: {script.FileName} was not run because migrations was already executed");
                            break;
                        default:
                            break;//ignore this one....
                    }
                }
                else if (result.IsFailure)
                {
                    _logger.LogError(result.Exception, $"Exception is thrown while running script: {script.FileName}");
                    return false;
                }
            }
            return true;
        }
    }
}
