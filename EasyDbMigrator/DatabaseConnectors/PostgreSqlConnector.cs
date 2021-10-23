using Npgsql;
using Polly;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest that will not be included in code coverage
    public class PostgreSqlConnector : IDatabaseConnector
    {
        private AsyncPolicy _postgreSqlDatabasePolicy = Policy.Handle<Exception>()
             .WaitAndRetryAsync(retryCount: 3
            , sleepDurationProvider: times => TimeSpan.FromSeconds(times * 1));

        public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {
            string query = $@"
               DROP DATABASE IF EXISTS  {migrationConfiguration.DatabaseName}
                ";

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
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
                Result<RunMigrationResult> result = await _postgreSqlDatabasePolicy.ExecuteAsync(async () =>
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
                    await transaction.DisposeAsync();

                    return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted);
                });

                return result;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        await transaction.RollbackAsync(cancellationToken: cancellationToken);
                        await transaction.DisposeAsync();
                        return new Result<RunMigrationResult>(isSucces: true
                            , RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted
                            , exception: ex);
                    }
                    catch (Exception ex2)
                    {
                        return new Result<RunMigrationResult>(isSucces: true
                            , RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted
                            , exception: new ApplicationException($"{ex} + {ex2.Message}"));
                    }
                }

                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, ex);
            }
        }

        private async Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString
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

                await _postgreSqlDatabasePolicy.ExecuteAsync(async () =>
                {
                    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync(cancellationToken);

                    using NpgsqlCommand command = new NpgsqlCommand(sqlScriptContent, connection);

                    _ = await command.ExecuteNonQueryAsync(cancellationToken);
                });

                return new Result<bool>(isSucces: true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(isSucces: false, exception: ex);
            }
        }
    }
}
