using Npgsql;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest that will not be included in code coverage
    public class PostgreSqlConnector : IDatabaseConnector
    {
        public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(string databaseName, string connectionString)
        {
            string query = $@"
               DROP DATABASE IF EXISTS  {databaseName}
                ";

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: connectionString
                 , scriptName: "EasyDbMigrator.Integrationtest_dropDatabase"
                 , sqlScriptContent: query);

            return result;
        }

        public async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(MigrationConfiguration migrationConfiguration)
        {
            string sqlScriptCreateMigrationTable = @$" 

                CREATE TABLE IF NOT EXISTS DbMigrationsRun (
                    Id          SERIAL PRIMARY KEY,
                    Executed    Timestamp NOT NULL,
                    Filename    varchar NOT NULL,
                    Version     varchar NOT NULL
                );
                ";

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                , scriptName: "EasyDbMigrator.SetupDbMigrationsRunTable"
                , sqlScriptContent: sqlScriptCreateMigrationTable);

            return result;
        }

        public async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration)
        {
            string sqlScriptCreateDatabase = @$"
                    SELECT 'CREATE DATABASE {migrationConfiguration.DatabaseName}'
                    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '{migrationConfiguration.DatabaseName}')
                    ";

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                , scriptName: "SetupEmptyDb"
                , sqlScriptContent: sqlScriptCreateDatabase);

            return result;
        }

        public async Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString
          , string scriptName
          , string sqlScriptContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sqlScriptContent))
                {
                    throw new ArgumentException($"{scriptName} script is empty, is there something wrong?");
                }

                using NpgsqlConnection connection = new NpgsqlConnection(connectionString);

                using NpgsqlCommand command = new NpgsqlCommand(sqlScriptContent, connection);

                await command.Connection.OpenAsync();
                _ = await command.ExecuteNonQueryAsync();

                return new Result<bool>(isSucces: true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(isSucces: false, exception: ex);
            }
        }
        public async Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(MigrationConfiguration migrationConfiguration
            , Script script
            , DateTimeOffset executedDateTime)
        {
            NpgsqlTransaction? transaction = null;
            try
            {
                using NpgsqlConnection connection = new NpgsqlConnection(migrationConfiguration.ConnectionString);
                await connection.OpenAsync();

                string checkIfScriptHasExecuted = $@"
                        SELECT Id
                        FROM DbMigrationsRun
                        WHERE Filename = '{script.FileName}'

                        ";

                using NpgsqlCommand cmdcheckNotExecuted = new NpgsqlCommand(checkIfScriptHasExecuted, connection);
                var result = _ = await cmdcheckNotExecuted.ExecuteScalarAsync();

                if (result != null)
                {
                    return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                }

                string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string updateVersioningTableScript = $@" 
                            
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                transaction = connection.BeginTransaction();//TODO make async with cancellationtoken if works

                using NpgsqlCommand cmdScript = new NpgsqlCommand(script.Content
                    , connection
                    , transaction);
                using NpgsqlCommand cmdUpdateVersioningTable = new NpgsqlCommand(updateVersioningTableScript, connection, transaction);

                _ = await cmdScript.ExecuteNonQueryAsync();
                _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync();

                transaction.Commit();//TODO make async with cancellationtoken if works

                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted);
            }
            catch (Exception ex)
            {
                transaction?.Rollback();//TODO make async with cancellationtoken if works
                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, ex);
            }
        }
    }
}
