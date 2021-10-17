using Npgsql;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest that will not be included in code coverage
    public class PostgreSqlConnector : IDatabaseConnector
    {
        public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(string databaseName
            , string connectionString
            , CancellationToken cancellationToken)
        {
            string query = $@"
               DROP DATABASE IF EXISTS  {databaseName}
                ";

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: connectionString
                 , scriptName: "EasyDbMigrator.Integrationtest_dropDatabase"
                 , sqlScriptContent: query
                 , cancellationToken: cancellationToken);

            return result;
        }

        public async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
                return new Result<bool>(isSucces: true);

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
                , sqlScriptContent: sqlScriptCreateMigrationTable
                , cancellationToken: cancellationToken);

            return result;
        }

        public async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
                return new Result<bool>(isSucces: true);

            string sqlScriptCreateDatabase = @$"
                    SELECT 'CREATE DATABASE {migrationConfiguration.DatabaseName}'
                    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '{migrationConfiguration.DatabaseName}')
                    ";

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                , scriptName: "SetupEmptyDb"
                , sqlScriptContent: sqlScriptCreateDatabase
                , cancellationToken: cancellationToken);

            return result;
        }

        public async Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString
          , string scriptName
          , string sqlScriptContent
          , CancellationToken cancellationToken)
          
        {
            if (cancellationToken.IsCancellationRequested)
                return new Result<bool>(isSucces: true);

            try
            {
                if (string.IsNullOrWhiteSpace(sqlScriptContent))
                {
                    throw new ArgumentException($"{scriptName} script is empty, is there something wrong?");
                }

                using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                using NpgsqlCommand command = new NpgsqlCommand(sqlScriptContent, connection);

                _ = await command.ExecuteNonQueryAsync(cancellationToken);

                return new Result<bool>(isSucces: true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(isSucces: false, exception: ex);
            }
        }
        public async Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(MigrationConfiguration migrationConfiguration
            , Script script
            , DateTimeOffset executedDateTime
            , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationWasCancelled);

            NpgsqlTransaction? transaction = null;
            try
            {
                using NpgsqlConnection connection = new NpgsqlConnection(migrationConfiguration.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                string checkIfScriptHasExecuted = $@"
                        SELECT Id
                        FROM DbMigrationsRun
                        WHERE Filename = '{script.FileName}'

                        ";

                using NpgsqlCommand cmdcheckNotExecuted = new NpgsqlCommand(checkIfScriptHasExecuted, connection);
                var result = _ = await cmdcheckNotExecuted.ExecuteScalarAsync(cancellationToken);

                if (result != null)
                {
                    return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                }

                string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string updateVersioningTableScript = $@" 
                            
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                transaction = await connection.BeginTransactionAsync(cancellationToken);

                using NpgsqlCommand cmdScript = new NpgsqlCommand(script.Content
                    , connection
                    , transaction);
                using NpgsqlCommand cmdUpdateVersioningTable = new NpgsqlCommand(updateVersioningTableScript, connection, transaction);

                _ = await cmdScript.ExecuteNonQueryAsync(cancellationToken);
                _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync(cancellationToken);

               await transaction.CommitAsync(cancellationToken);

                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    await transaction.DisposeAsync();
                }
                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, ex);
            }
        }
    }
}
