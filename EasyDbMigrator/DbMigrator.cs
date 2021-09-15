using EasyDbMigrator.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public class DbMigrator
    {
        public DbMigrator()
        {
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

            await SetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(sqlDataBaseInfo: sqlDataBaseInfo); //TODO do some reporting back when fails and unittest this
            await SetupMigrationTableWhenNotExcistAsync(sqlDataBaseInfo: sqlDataBaseInfo); //TODO do some reporting back when fails and unittest this
            await RunMigrationScriptsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                , customclassType: customClass
                , executedDateTime: executedDateTime);//TODO do some reporting back when fails and unittest this

            return true;
        }

        private async Task SetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(SqlDataBaseInfo sqlDataBaseInfo)
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

        private async Task SetupMigrationTableWhenNotExcistAsync(SqlDataBaseInfo sqlDataBaseInfo)
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

        private async Task RunMigrationScriptsAsync(SqlDataBaseInfo sqlDataBaseInfo
            , Type customclassType
            , DateTime executedDateTime)
        {
            ScriptsHelper scriptsHelper = new ScriptsHelper();
            List<Script> orderedScripts = await scriptsHelper.TryConvertoScriptsInCorrectSequenceByTypeAsync(customclassType);

            foreach (Script script in orderedScripts)
            {
                _ = await new SqlDbHelper().RunDbMigrationScriptAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , script: script
                    , executedDateTime: executedDateTime);
            }
        }

       
    }
}
