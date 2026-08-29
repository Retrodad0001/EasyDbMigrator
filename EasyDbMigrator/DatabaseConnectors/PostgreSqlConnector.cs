// Ignore Spelling: Postgre Sql

using Npgsql;
using Polly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors;

/// <summary>
/// Connects to a PostgreSql database
/// </summary>
public sealed class PostgreSqlConnector : IDatabaseConnector
{
    private readonly AsyncPolicy _postgreSqlDatabasePolicy = Policy.Handle<Exception>()
         .WaitAndRetryAsync(retryCount: 3
        , sleepDurationProvider: times => TimeSpan.FromSeconds(value: times * 2));

    /// <summary>
    /// Tries to delete the database if it exists. Postgres refuses to drop a
    /// database that still has sessions attached, so the pooled connections are
    /// released and any remaining backend is terminated first. This mirrors the
    /// SET OFFLINE WITH ROLLBACK IMMEDIATE that MicrosoftSqlConnector uses.
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<bool>(wasSuccessful: true);
        }

        try
        {
            await using (NpgsqlConnection pooledConnection = new(connectionString: BuildMigrationDatabaseConnectionString(migrationConfiguration)))
            {
                NpgsqlConnection.ClearPool(pooledConnection);
            }

            await _postgreSqlDatabasePolicy.ExecuteAsync(action: async () =>
            {
                await using NpgsqlConnection connection = new(connectionString: migrationConfiguration.ConnectionString);
                await connection.OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                await using NpgsqlCommand terminateOtherSessions = new(
                    cmdText: @"
                        SELECT pg_terminate_backend(pid)
                        FROM pg_stat_activity
                        WHERE datname = @databaseName AND pid <> pg_backend_pid()"
                    , connection: connection);
                _ = terminateOtherSessions.Parameters.AddWithValue("databaseName", migrationConfiguration.DatabaseName);

                _ = await terminateOtherSessions.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                await using NpgsqlCommand dropDatabase = new(
                    cmdText: $@"DROP DATABASE IF EXISTS ""{migrationConfiguration.DatabaseName}"""
                    , connection: connection);

                _ = await dropDatabase.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }).ConfigureAwait(continueOnCapturedContext: false);

            return new Result<bool>(wasSuccessful: true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(wasSuccessful: false, exception: ex);
        }
    }

    /// <summary>
    /// Tries to create the versioning table inside the database named by
    /// <see cref="MigrationConfiguration.DatabaseName"/> when it does not exist yet
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {

        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<bool>(wasSuccessful: true);
        }

        const string sqlScriptCreateMigrationTable = @$"

                CREATE TABLE IF NOT EXISTS DbMigrationsRun (
                    Id          SERIAL PRIMARY KEY,
                    Executed    Timestamp NOT NULL,
                    Filename    varchar NOT NULL,
                    Version     varchar NOT NULL
                );
                ";

        var result = await TryExecuteSingleScriptAsync(connectionString: BuildMigrationDatabaseConnectionString(migrationConfiguration)
            , sqlScriptContent: sqlScriptCreateMigrationTable
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        return result;
    }

    /// <summary>
    /// Tries to create the database named by <see cref="MigrationConfiguration.DatabaseName"/>
    /// when it does not exist yet. The connection string is expected to carry no
    /// Database of its own, so this connects to the server default and creates the
    /// migration database from there.
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {

        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<bool>(wasSuccessful: true);
        }

        try
        {
            await _postgreSqlDatabasePolicy.ExecuteAsync(action: async () =>
            {
                await using NpgsqlConnection connection = new(connectionString: migrationConfiguration.ConnectionString);
                await connection.OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                await using NpgsqlCommand checkDatabaseExists = new(
                    cmdText: "SELECT 1 FROM pg_database WHERE datname = @databaseName"
                    , connection: connection);
                _ = checkDatabaseExists.Parameters.AddWithValue("databaseName", migrationConfiguration.DatabaseName);

                object? alreadyExists = await checkDatabaseExists.ExecuteScalarAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                if (alreadyExists is not null)
                {
                    return;
                }

                await using NpgsqlCommand createDatabase = new(
                    cmdText: $@"CREATE DATABASE ""{migrationConfiguration.DatabaseName}"""
                    , connection: connection);

                _ = await createDatabase.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }).ConfigureAwait(continueOnCapturedContext: false);

            return new Result<bool>(wasSuccessful: true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(wasSuccessful: false, exception: ex);
        }
    }

    private static string BuildMigrationDatabaseConnectionString(MigrationConfiguration migrationConfiguration)
    {
        NpgsqlConnectionStringBuilder builder = new(migrationConfiguration.ConnectionString)
        {
            Database = migrationConfiguration.DatabaseName
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// Runs a migration script
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="script"></param>
    /// <param name="executedDateTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<RunMigrationResult>> RunDbMigrationScriptAsync(MigrationConfiguration migrationConfiguration
        , Script script
        , DateTimeOffset executedDateTime
        , CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationWasCancelled);
        }

        NpgsqlTransaction? transaction = null;
        try
        {
            var result = await _postgreSqlDatabasePolicy.ExecuteAsync(action: async () =>
            {

                await using NpgsqlConnection connection = new(connectionString: BuildMigrationDatabaseConnectionString(migrationConfiguration));
                await connection.OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                string checkIfScriptHasExecuted = $@"
                        SELECT Id
                        FROM DbMigrationsRun
                        WHERE Filename = '{script.FileName}'

                        ";

                await using NpgsqlCommand cmdCheckNotExecuted = new(cmdText: checkIfScriptHasExecuted, connection: connection);
                object? result = _ = await cmdCheckNotExecuted.ExecuteScalarAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                if (result != null)
                {
                    return new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                }

                string sqlFormattedDate = executedDateTime.ToString(format: "yyyy-MM-dd HH:mm:ss");
                string updateVersioningTableScript = $@"
                            
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                transaction = await connection.BeginTransactionAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                await using NpgsqlCommand cmdScript = new(cmdText: script.Content
                    , connection: connection
                    , transaction: transaction);
                await using NpgsqlCommand cmdUpdateVersioningTable = new(cmdText: updateVersioningTableScript, connection: connection, transaction: transaction);

                _ = await cmdScript.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                await transaction.CommitAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                await transaction.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);

                return new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationScriptExecuted);
            }).ConfigureAwait(continueOnCapturedContext: false);

            return result;
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                try
                {
                    await transaction.RollbackAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    await transaction.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
                    return new Result<RunMigrationResult>(wasSuccessful: true
                        , value: RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted
                        , exception: ex);
                }
                catch (Exception ex2)
                {
                    return new Result<RunMigrationResult>(wasSuccessful: true
                        , value: RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted
                        , exception: new ApplicationException(message: $"{ex} + {ex2.Message}"));
                }
            }

            return new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted, exception: ex);
        }
    }

    private async Task<Result<bool>> TryExecuteSingleScriptAsync(string connectionString
     , string sqlScriptContent
     , CancellationToken cancellationToken)

    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<bool>(wasSuccessful: true);
        }

        try
        {
            await _postgreSqlDatabasePolicy.ExecuteAsync(action: async () =>
            {
                await using NpgsqlConnection connection = new(connectionString: connectionString);
                await connection.OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                await using NpgsqlCommand command = new(cmdText: sqlScriptContent, connection: connection);

                _ = await command.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }).ConfigureAwait(continueOnCapturedContext: false);

            return new Result<bool>(wasSuccessful: true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(wasSuccessful: false, exception: ex);
        }
    }
}
