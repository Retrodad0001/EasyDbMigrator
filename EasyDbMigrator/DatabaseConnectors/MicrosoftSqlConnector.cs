using Polly;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest that will not be included in code coverage
    public class MicrosoftSqlConnector : IDatabaseConnector
    {
        private readonly AsyncPolicy _sqlDatabasePolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(retryCount: 3
            , sleepDurationProvider: times => TimeSpan.FromSeconds(times * 1));

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

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                   , scriptName: "EasyDbMigrator.Integrationtest_dropDatabase"
                   , sqlScriptContent: query
                   , cancellationToken: cancellationToken).ConfigureAwait(true);
            return result;
        }

        public async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(MigrationConfiguration migrationConfiguration
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

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                , scriptName: "EasyDbMigrator.SetupDbMigrationsRunTable"
                , sqlScriptContent: sqlScriptCreateMigrationTable
                , cancellationToken: cancellationToken).ConfigureAwait(false);

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

            Result<bool> result = await TryExcecuteSingleScriptAsync(connectionString: migrationConfiguration.ConnectionString
                , scriptName: "SetupEmptyDb"
                , sqlScriptContent: sqlScriptCreateDatabase
                , cancellationToken: cancellationToken).ConfigureAwait(true); ;

            return result;
        }

        public async Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(MigrationConfiguration migrationConfiguration
            , Script script
            , DateTimeOffset executedDateTime
            , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationWasCancelled);

            SqlTransaction? transaction = null;
            try
            {
                Result<RunMigrationResult> result = await _sqlDatabasePolicy.ExecuteAsync(async () =>
                {
                    using SqlConnection connection = new SqlConnection(migrationConfiguration.ConnectionString);
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(true);

                    string checkIfScriptHasExecuted = $@"USE {migrationConfiguration.DatabaseName} 
                        SELECT Id
                        FROM DbMigrationsRun
                         WHERE Filename = '{script.FileName}'
                        ";

                    using SqlCommand cmdcheckNotExecuted = new SqlCommand(checkIfScriptHasExecuted, connection);
                    var result = _ = await cmdcheckNotExecuted.ExecuteScalarAsync(cancellationToken).ConfigureAwait(true);

                    if (result != null)
                    {
                        return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                    }

                    string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz");
                    string updateVersioningTableScript = $@" 
                            USE {migrationConfiguration.DatabaseName} 
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                    transaction = await connection.BeginTransactionAsync(isolationLevel: IsolationLevel.Serializable
                        , cancellationToken: cancellationToken).ConfigureAwait(true) as SqlTransaction;

                    using SqlCommand cmdScript = new SqlCommand(script.Content, connection, transaction);
                    using SqlCommand cmdUpdateVersioningTable = new SqlCommand(updateVersioningTableScript, connection, transaction);

                    _ = await cmdScript.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(true);
                    _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync(cancellationToken: cancellationToken).ConfigureAwait(true);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(true);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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

                await _sqlDatabasePolicy.ExecuteAsync(async () =>
                {
                    using SqlConnection connection = new SqlConnection(connectionString);
                    using SqlCommand command = new SqlCommand(sqlScriptContent, connection);

                    await command.Connection.OpenAsync(cancellationToken).ConfigureAwait(true);
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
