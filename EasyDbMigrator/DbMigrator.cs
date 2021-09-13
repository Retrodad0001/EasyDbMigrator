using EasyDbMigrator.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public class DbMigrator
    {
        private readonly string _connectionstring;

        public DbMigrator(string connectionstring)
        {
            if (string.IsNullOrWhiteSpace(connectionstring))
            {
                throw new ArgumentException($"'{nameof(connectionstring)}' cannot be null or whitespace.", nameof(connectionstring));
            }

            _connectionstring = connectionstring;
        }

        public async Task<bool> TryApplyMigrationsAsync(string databasename, Type customClass, DateTime executedDateTime)  
        {
            if (string.IsNullOrWhiteSpace(databasename)) 
                throw new ArgumentException($"'{nameof(databasename)}' cannot be null or whitespace.", nameof(databasename));

            await SetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(databasename: databasename); //TODO do some reporting back when fails and unittest this
            await SetupMigrationTableWhenNotExcistAsync(databasename: databasename); //TODO do some reporting back when fails and unittest this
            await RunMigrationScriptsAsync(databasename: databasename
                , customclassType: customClass
                , executedDateTime: executedDateTime);//TODO do some reporting back when fails and unittest this

            return true;
        }

        private async Task SetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(string databasename)
        {
            string sqlScriptCreateDatabase = @$" 
                USE Master
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{databasename}')
                BEGIN
                    CREATE DATABASE {databasename}
                END";

            _ = await TryExcecuteScriptAsync(string.Empty, sqlScriptContent: sqlScriptCreateDatabase);
        }

        private async Task SetupMigrationTableWhenNotExcistAsync(string databasename)
        {
            string sqlScriptCreateMigrationTable = @$" USE {databasename}  
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DbMigrationsRun' AND xtype='U')
                BEGIN
                    CREATE TABLE DbMigrationsRun 
                    (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Executed Datetime2 NOT NULL,
                        ScriptName nvarchar(50) NOT NULL UNIQUE,
                        ScriptContent nvarchar(max) NOT NULL,
                        Version nvarchar(10) NOT NULL
                    )
                END";

            _ = await TryExcecuteScriptAsync(string.Empty, sqlScriptContent: sqlScriptCreateMigrationTable);
        }

        private async Task RunMigrationScriptsAsync(string databasename, Type customclassType, DateTime executedDateTime)
        {
            ScriptsHelper scriptsHelper = new ScriptsHelper();
            List<Script> orderedScripts = await scriptsHelper.TryConvertoScriptsInCorrectSequenceByTypeAsync(customclassType);

            //TODO test: script-name should be unique

            string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (Script script in orderedScripts)
            {
                //TODO check if script has already run based on NameScriptsComplete exist in table

                string sqlscriptToExecute = string.Empty;
                sqlscriptToExecute += " BEGIN TRANSACTION;";
                sqlscriptToExecute += script.Content;
                sqlscriptToExecute += @$" USE {databasename} 
                            INSERT INTO DbMigrationsRun (Executed, ScriptName, ScriptContent, version)
                            VALUES ('{sqlFormattedDate}', '{script.NameScriptsComplete}', 'xx', '1.0.0');
                        ";
                sqlscriptToExecute += " COMMIT;";

                _ = await TryExcecuteScriptAsync(scriptname: script.NameScriptsComplete, sqlScriptContent: sqlscriptToExecute);
            }
        }

        private async Task<bool> TryExcecuteScriptAsync(string scriptname, string sqlScriptContent)
        {
                if (string.IsNullOrWhiteSpace(sqlScriptContent))
                {
                    throw new ArgumentException($"{scriptname} script is empty, is there something wrong?");
                }

                using SqlConnection connection = new SqlConnection(_connectionstring);
                using SqlCommand command = new(sqlScriptContent, connection);

                await command.Connection.OpenAsync();
                _ = await command.ExecuteNonQueryAsync();
           
            return true;
        }
    }
}
