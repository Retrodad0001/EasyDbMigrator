using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest that will not be included in code coverage
    public class MicrosoftSqlConnector : IDatabaseConnector
    {
        public async Task<Result<bool>> TryDeleteDatabaseIfExistAsync(string databaseName
            , string connectionString
            , CancellationToken cancellationToken)
        {

            string query = $@"
                IF EXISTS(SELECT * FROM master.sys.databases WHERE name='{databaseName}')
                BEGIN               
                    ALTER DATABASE {databaseName} 
                    SET OFFLINE WITH ROLLBACK IMMEDIATE;
                    ALTER DATABASE {databaseName} SET ONLINE;
                    DROP DATABASE {databaseName};
                END
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
                , cancellationToken: cancellationToken);

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
                , cancellationToken: cancellationToken);

            return result;
        }

        public async Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString
            , string scriptName
            , string sqlScriptContent
            , CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sqlScriptContent))
                {
                    throw new ArgumentException($"{scriptName} script is empty, is there something wrong?");
                }

                using SqlConnection connection = new SqlConnection(connectionString);
                using SqlCommand command = new SqlCommand(sqlScriptContent, connection);

                await command.Connection.OpenAsync(cancellationToken);
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
            
            SqlTransaction? transaction = null;
            try
            {
                using SqlConnection connection = new SqlConnection(migrationConfiguration.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                string checkIfScriptHasExecuted = $@"USE {migrationConfiguration.DatabaseName} 
                        SELECT Id
                        FROM DbMigrationsRun
                         WHERE Filename = '{script.FileName}'
                        ";

                using SqlCommand cmdcheckNotExecuted = new SqlCommand(checkIfScriptHasExecuted, connection);
                var result = _ = await cmdcheckNotExecuted.ExecuteScalarAsync(cancellationToken);

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

                transaction =  await connection.BeginTransactionAsync(isolationLevel: IsolationLevel.Serializable
                    , cancellationToken: cancellationToken) as SqlTransaction;

                using SqlCommand cmdScript = new SqlCommand(script.Content, connection, transaction);
                using SqlCommand cmdUpdateVersioningTable = new SqlCommand(updateVersioningTableScript, connection, transaction);

                _ = await cmdScript.ExecuteNonQueryAsync(cancellationToken: cancellationToken);
                _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync(cancellationToken: cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken: cancellationToken);
                    await transaction.DisposeAsync();
                }
                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, ex);
            }
        }

       
    }
}
