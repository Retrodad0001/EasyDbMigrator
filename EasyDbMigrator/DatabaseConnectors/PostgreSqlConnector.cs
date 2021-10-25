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
        private readonly AsyncPolicy _postgreSqlDatabasePolicy = Policy.Handle<Exception>()
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
                 , cancellationToken: cancellationToken).ConfigureAwait(true); ;

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
                , cancellationToken: cancellationToken).ConfigureAwait(true);

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
                , cancellationToken: cancellationToken).ConfigureAwait(true);

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
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(true);

                    string checkIfScriptHasExecuted = $@"
                        SELECT Id
                        FROM DbMigrationsRun
                        WHERE Filename = '{script.FileName}'

                        ";

                    using NpgsqlCommand cmdcheckNotExecuted = new NpgsqlCommand(checkIfScriptHasExecuted, connection);
                    var result = _ = await cmdcheckNotExecuted.ExecuteScalarAsync(cancellationToken).ConfigureAwait(true);

                    if (result != null)
                    {
                        return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                    }

                    string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string updateVersioningTableScript = $@" 
                            
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                    transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(true);

                    using NpgsqlCommand cmdScript = new NpgsqlCommand(script.Content
                        , connection
                        , transaction);
                    using NpgsqlCommand cmdUpdateVersioningTable = new NpgsqlCommand(updateVersioningTableScript, connection, transaction);

                    _ = await cmdScript.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(true);
                    _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(true);

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(true);
                    await transaction.DisposeAsync().ConfigureAwait(true);

                    return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted);
                }).ConfigureAwait(true);

                return result;
            }
            catch (Exception ex)
            {
#pragma warning disable CA1508 // Avoid dead conditional code
                if (transaction != null)
#pragma warning restore CA1508 // Avoid dead conditional code
                {
                    try
                    {
                        await transaction.RollbackAsync(cancellationToken: cancellationToken).ConfigureAwait(true);
                        await transaction.DisposeAsync().ConfigureAwait(true);
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
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(true);

                    using NpgsqlCommand command = new NpgsqlCommand(sqlScriptContent, connection);

                    _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(true);
                }).ConfigureAwait(true);

                return new Result<bool>(isSucces: true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(isSucces: false, exception: ex);
            }
        }
    }
}
