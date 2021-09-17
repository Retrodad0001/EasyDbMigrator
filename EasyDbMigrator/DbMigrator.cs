using EasyDbMigrator.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public class DbMigrator
    {
        private readonly ILogger _logger;

        public DbMigrator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 

          
        }

        public async Task<bool> TryApplyMigrationsAsync(SqlDataBaseInfo sqlDataBaseInfo
            , Type customClass
            , DateTime executedDateTime)
        {
            if (sqlDataBaseInfo is null)
            {
                throw new ArgumentNullException(nameof(sqlDataBaseInfo));
            }

            if (customClass is null) //TODO: change this because this is not needed when run form command-line
            {
                throw new ArgumentNullException(nameof(customClass));
            }

            await TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(sqlDataBaseInfo: sqlDataBaseInfo); 
            _logger.LogInformation(@"setup database when there is none with default settings executed successfully");
            
            await TrySetupDbMigrationsRunTableWhenNotExcistAsync(sqlDataBaseInfo: sqlDataBaseInfo); 
            _logger.LogInformation(@"setup DbMigrationsRun when there is none executed successfully");
            
            await TryRunAllMigrationScriptsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                , customclassType: customClass
                , executedDateTime: executedDateTime);
            _logger.LogInformation("Whole migration process executed successfully");
            
            return true;
        }

        private async Task TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(SqlDataBaseInfo sqlDataBaseInfo)
        {
            string sqlScriptCreateDatabase = @$" 
                USE Master
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{sqlDataBaseInfo.DatabaseName}')
                BEGIN
                    CREATE DATABASE {sqlDataBaseInfo.DatabaseName}
                END";

            _ = await new SqlDbHelper().TryExcecuteSingleScriptAsync(connectionString: sqlDataBaseInfo.ConnectionString
                , scriptName: "EasyDbMigrator.SetupEmptyDb"
                , sqlScriptContent: sqlScriptCreateDatabase);
        }

        private async Task TrySetupDbMigrationsRunTableWhenNotExcistAsync(SqlDataBaseInfo sqlDataBaseInfo)
        {
            string sqlScriptCreateMigrationTable = @$" USE {sqlDataBaseInfo.DatabaseName}  
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DbMigrationsRun' AND xtype='U')
                BEGIN
                    CREATE TABLE DbMigrationsRun 
                    (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Executed Datetime2 NOT NULL,
                        ScriptName nvarchar(50) NOT NULL UNIQUE,
                        Version nvarchar(10) NOT NULL
                    )
                END";

            _ = await new SqlDbHelper().TryExcecuteSingleScriptAsync(connectionString: sqlDataBaseInfo.ConnectionString
                , scriptName: "EasyDbMigrator.SetupDbMigrationsRunTable"
                , sqlScriptContent: sqlScriptCreateMigrationTable);
        }

        private async Task TryRunAllMigrationScriptsAsync(SqlDataBaseInfo sqlDataBaseInfo
            , Type customclassType
            , DateTime executedDateTime)
        {
            ScriptsHelper scriptsHelper = new ScriptsHelper();
            List<Script> orderedScripts = await scriptsHelper.TryConvertoScriptsInCorrectSequenceByTypeAsync(customclassType);

            foreach (Script script in orderedScripts)
            {
                Result<RunMigrationResult> result = await new SqlDbHelper().RunDbMigrationScriptWhenNotRunnedBeforeAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , script: script
                    , executedDateTime: executedDateTime);

                if (result.IsSuccess)
                {
                    switch (result.Value)
                    {
                        case RunMigrationResult.MigrationScriptExecuted:
                            _logger.LogInformation($"script: {script.NamePart} was run");
                            break;
                        case RunMigrationResult.IgnoredAllreadyRun:
                            _logger.LogInformation($"script: {script.NamePart} was not run because migrations was already executed");
                            break;
                        default:
                            break;//ignore this one....
                    }
                }
                else if (result.IsFailure)
                {
                    _logger.LogError(result.Exception, $"Exception is thrown while running script: {script.NamePart}");
                }
            }
        }

    }
}
