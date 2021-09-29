using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest that will not be included in code coverage
    public class SqlDbConnector : IDatabaseConnector
    {
        public async Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sqlScriptContent))
                {
                    throw new ArgumentException($"{scriptName} script is empty, is there something wrong?");
                }

                using SqlConnection connection = new SqlConnection(connectionString);
                using SqlCommand command = new(sqlScriptContent, connection);

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
            , SqlScript script
            , DateTime executedDateTime)
        {
            SqlTransaction? transaction = null;
            try
            {
                using SqlConnection connection = new SqlConnection(migrationConfiguration.ConnectionString);
                await connection.OpenAsync();
                //check if script was executed before

                string checkIfScriptHasExecuted = $@"USE {migrationConfiguration.DatabaseName} 
                        SELECT Id
                        FROM DbMigrationsRun
                         WHERE Filename = '{script.FileName}'
                        ";

                using SqlCommand cmdcheckNotExecuted = new(checkIfScriptHasExecuted, connection);
                var result = _ = await cmdcheckNotExecuted.ExecuteScalarAsync();

                if (result is not null)
                {
                    return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun);
                }

                string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string updateVersioningTableScript = $@" 
                            USE {migrationConfiguration.DatabaseName} 
                            INSERT INTO DbMigrationsRun (Executed, Filename, version)
                            VALUES ('{sqlFormattedDate}', '{script.FileName}', '1.0.0');
                        ";

                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "EasyDbMigrator");

                using SqlCommand cmdScript = new(script.Content, connection, transaction);
                using SqlCommand cmdUpdateVersioningTable = new(updateVersioningTableScript, connection, transaction);

                _ = await cmdScript.ExecuteNonQueryAsync();
                _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync();

                transaction.Commit();

                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted);
            }
            catch (SqlException ex)
            {
                transaction?.Rollback();
                return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, ex);
            }
        }
    }
}
