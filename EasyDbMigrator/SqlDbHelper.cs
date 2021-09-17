using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EasyDbMigrator.Helpers
{
    public partial class SqlDbHelper
    {
        public async Task<bool> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent)
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

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(SqlDataBaseInfo sqlDataBaseInfo
            , Script script
            , DateTime executedDateTime)
        {
            SqlTransaction? transaction = null;
            try
            {
                using SqlConnection connection = new SqlConnection(sqlDataBaseInfo.ConnectionString);
                await connection.OpenAsync();
                //check if script was executed before

                string checkIfScriptHasExecuted = $@"USE {sqlDataBaseInfo.DatabaseName} 
                        SELECT Id
                        FROM DbMigrationsRun
                         WHERE ScriptName = '{script.NamePart}'
                        ";

                using SqlCommand cmdcheckNotExecuted = new(checkIfScriptHasExecuted, connection);
                var result = _ = await cmdcheckNotExecuted.ExecuteScalarAsync();

                if (result is not null)
                {
                    return new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.IgnoredAllreadyRun);
                }

                string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");//TODO find better way than this way
                string updateVersioningTableScript = $@" 
                            USE {sqlDataBaseInfo.DatabaseName} 
                            INSERT INTO DbMigrationsRun (Executed, ScriptName, version)
                            VALUES ('{sqlFormattedDate}', '{script.NamePart}', '1.0.0');
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
                return new Result<RunMigrationResult>(isSucces: false, ex);
            }
        }
    }
}
