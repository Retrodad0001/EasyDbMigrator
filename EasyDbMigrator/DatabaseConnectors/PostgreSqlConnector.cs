using Npgsql;
using Polly;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors;

[ExcludeFromCodeCoverage] //is tested with integrationTest that will not be included in code coverage
public sealed class PostgreSqlConnector : IDatabaseConnector
{
    private readonly AsyncPolicy _postgreSqlDatabasePolicy = Policy.Handle<Exception>()
         .WaitAndRetryAsync(retryCount: 3
        , sleepDurationProvider: times => TimeSpan.FromSeconds(value: times * 2));

    public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {
        string query = $@"
               DROP DATABASE IF EXISTS  {migrationConfiguration.DatabaseName}
                ";

        var result = await TryExecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
             , scriptName: @"EasyDbMigrator.Integrationtest_dropDatabase"
             , sqlScriptContent: query
             , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        return result;
    }

    public async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {

        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<bool>(wasSuccessful: true);
        }

        string sqlScriptCreateMigrationTable = @$" 

                CREATE TABLE IF NOT EXISTS DbMigrationsRun (
                    Id          SERIAL PRIMARY KEY,
                    Executed    Timestamp NOT NULL,
                    Filename    varchar NOT NULL,
                    Version     varchar NOT NULL
                );
                ";

        var result = await TryExecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
            , scriptName: "EasyDbMigrator.SetupDbMigrationsRunTable"
            , sqlScriptContent: sqlScriptCreateMigrationTable
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        return result;
    }

    public async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {

        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<bool>(wasSuccessful: true);
        }

        // ReSharper disable once StringLiteralTypo
        string sqlScriptCreateDatabase = @$"
                    SELECT 'CREATE DATABASE {migrationConfiguration.DatabaseName}'
                    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '{migrationConfiguration.DatabaseName}')
                    ";

        var result = await TryExecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
            , scriptName: "SetupEmptyDb"
            , sqlScriptContent: sqlScriptCreateDatabase
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        return result;
    }

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

                await using NpgsqlConnection connection = new(connectionString: migrationConfiguration.ConnectionString);
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
     , string scriptName
     , string sqlScriptContent
     , CancellationToken cancellationToken)

    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new Result<bool>(wasSuccessful: true);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(value: sqlScriptContent))
            {
                throw new ArgumentException(message: $"{scriptName} script is empty, is there something wrong?");
            }

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
