using Npgsql;
using Polly;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors
{
    [ExcludeFromCodeCoverage] //is tested with integrationTest that will not be included in code coverage
    public sealed class PostgreSqlConnector : IDatabaseConnector
    {
        private readonly AsyncPolicy _postgreSqlDatabasePolicy = Policy.Handle<Exception>()
             .WaitAndRetryAsync(3
            , times => TimeSpan.FromSeconds(times * 2));

        public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {
            string query = $@"
               DROP DATABASE IF EXISTS  {migrationConfiguration.DatabaseName}
                ";

            var result = await TryExecuteSingleScriptAsync(migrationConfiguration.ConnectionString
                 , @"EasyDbMigrator.Integrationtest_dropDatabase"
                 , query
                 , cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
            {
                return new Result<bool>(true);
            }

            string sqlScriptCreateMigrationTable = @$" 

                CREATE TABLE IF NOT EXISTS DbMigrationsRun (
                    Id          SERIAL PRIMARY KEY,
                    Executed    Timestamp NOT NULL,
                    Filename    varchar NOT NULL,
                    Version     varchar NOT NULL
                );
                ";

            var result = await TryExecuteSingleScriptAsync(migrationConfiguration.ConnectionString
                , "EasyDbMigrator.SetupDbMigrationsRunTable"
                , sqlScriptCreateMigrationTable
                , cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
            {
                return new Result<bool>(true);
            }

            string sqlScriptCreateDatabase = @$"
                    SELECT 'CREATE DATABASE {migrationConfiguration.DatabaseName}'
                    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '{migrationConfiguration.DatabaseName}')
                    ";

            var result = await TryExecuteSingleScriptAsync(migrationConfiguration.ConnectionString
                , "SetupEmptyDb"
                , sqlScriptCreateDatabase
                , cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<Result<RunMigrationResult>> RunDbMigrationScriptAsync(MigrationConfiguration migrationConfiguration
            , Script script
            , DateTimeOffset executedDateTime
            , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new Result<RunMigrationResult>(true, RunMigrationResult.MigrationWasCancelled);
            }

            NpgsqlTransaction? transaction = null;
            try
            {
                var result = await _postgreSqlDatabasePolicy.ExecuteAsync(async () =>
                {

                    await using NpgsqlConnection connection = new(migrationConfiguration.ConnectionString);
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                    string checkIfScriptHasExecuted = $@"
                        SELECT Id
                        FROM DbMigrationsRun
                        WHERE Filename = '{script.FileName}'

                        ";

                    await using NpgsqlCommand cmdCheckNotExecuted = new(checkIfScriptHasExecuted, connection);
                    object? result = _ = await cmdCheckNotExecuted.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                    if (result != null)
                    {
                        return new Result<RunMigrationResult>(true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                    }

                    string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string updateVersioningTableScript = $@"
                            
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                    transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                    await using NpgsqlCommand cmdScript = new(script.Content
                        , connection
                        , transaction);
                    await using NpgsqlCommand cmdUpdateVersioningTable = new(updateVersioningTableScript, connection, transaction);

                    _ = await cmdScript.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.DisposeAsync().ConfigureAwait(false);

                    return new Result<RunMigrationResult>(true, RunMigrationResult.MigrationScriptExecuted);
                }).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                        await transaction.DisposeAsync().ConfigureAwait(false);
                        return new Result<RunMigrationResult>(true
                            , RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted
                            , ex);
                    }
                    catch (Exception ex2)
                    {
                        return new Result<RunMigrationResult>(true
                            , RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted
                            , new ApplicationException($"{ex} + {ex2.Message}"));
                    }
                }

                return new Result<RunMigrationResult>(true, RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted, ex);
            }
        }

        private async Task<Result<bool>> TryExecuteSingleScriptAsync(string connectionString
         , string scriptName
         , string sqlScriptContent
         , CancellationToken cancellationToken)

        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new Result<bool>(true);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(sqlScriptContent))
                {
                    throw new ArgumentException($"{scriptName} script is empty, is there something wrong?");
                }

                await _postgreSqlDatabasePolicy.ExecuteAsync(async () =>
                {
                    await using NpgsqlConnection connection = new(connectionString);
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                    await using NpgsqlCommand command = new(sqlScriptContent, connection);

                    _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }).ConfigureAwait(false);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, ex);
            }
        }
    }
}
