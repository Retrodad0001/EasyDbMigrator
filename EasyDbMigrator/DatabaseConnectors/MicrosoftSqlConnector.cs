// Ignore Spelling: Sql

using Polly;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors;

[ExcludeFromCodeCoverage] //is tested with integrationTest that will not be included in code coverage
public sealed class MicrosoftSqlConnector : IDatabaseConnector
{
    private readonly AsyncPolicy _sqlDatabasePolicy = Policy.Handle<Exception>()
        .WaitAndRetryAsync(retryCount: 3
        , sleepDurationProvider: times => TimeSpan.FromSeconds(value: times * 2));

    public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {
        string query = $@"
                IF EXISTS(SELECT * FROM master.sys.databases WHERE name='{migrationConfiguration.DatabaseName}')
                BEGIN               
                    ALTER DATABASE {migrationConfiguration.DatabaseName} 
                    SET OFFLINE WITH ROLLBACK IMMEDIATE;
                    ALTER DATABASE {migrationConfiguration.DatabaseName} SET ONLINE;
                    DROP DATABASE {migrationConfiguration.DatabaseName};
                END
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
        string sqlScriptCreateMigrationTable = @$" USE {migrationConfiguration.DatabaseName}  
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DbMigrationsRun' AND xtype='U')
                BEGIN
                    CREATE TABLE DbMigrationsRun 
                    (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Executed datetimeoffset NOT NULL,
                        Filename nvarchar(100) NOT NULL UNIQUE,
                        Version nvarchar(10) NOT NULL
                    )
                END";

        var result = await TryExecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
            , scriptName: "EasyDbMigrator.SetupDbMigrationsRunTable"
            , sqlScriptContent: sqlScriptCreateMigrationTable
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        return result;
    }

    public async Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration, CancellationToken cancellationToken)
    {
        string sqlScriptCreateDatabase = @$" 
                USE Master
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{migrationConfiguration.DatabaseName}')
                BEGIN
                    CREATE DATABASE {migrationConfiguration.DatabaseName}
                END";

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

        SqlTransaction? transaction = null;
        try
        {
            var result = await _sqlDatabasePolicy.ExecuteAsync(action: async () =>
            {
                await using SqlConnection connection = new(connectionString: migrationConfiguration.ConnectionString);
                await connection.OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                string checkIfScriptHasExecuted = $@"USE {migrationConfiguration.DatabaseName} 
                        SELECT Id
                        FROM DbMigrationsRun
                         WHERE Filename = '{script.FileName}'
                        ";

                await using SqlCommand cmdCheckNotExecuted = new(cmdText: checkIfScriptHasExecuted, connection: connection);
                object? result = _ = await cmdCheckNotExecuted.ExecuteScalarAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                if (result != null)
                {
                    return new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                }

                string sqlFormattedDate = executedDateTime.ToString(format: @"yyyy-MM-dd HH:mm:ss.fffffff zzz");
                string updateVersioningTableScript = $@" 
                            USE {migrationConfiguration.DatabaseName} 
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                transaction = await connection.BeginTransactionAsync(isolationLevel: IsolationLevel.Serializable
                    , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false) as SqlTransaction;

                await using SqlCommand cmdScript = new(cmdText: script.Content, connection: connection, transaction: transaction);
                await using SqlCommand cmdUpdateVersioningTable = new(cmdText: updateVersioningTableScript, connection: connection, transaction: transaction);

                _ = await cmdScript.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    await transaction.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
                }

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

            await _sqlDatabasePolicy.ExecuteAsync(action: async () =>
            {
                await using SqlConnection connection = new(connectionString: connectionString);
                await using SqlCommand command = new(cmdText: sqlScriptContent, connection: connection);

                await command.Connection.OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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
