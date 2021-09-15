using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EasyDbMigrator.Helpers
{
    public class SqlDbHelper
    {
        public async Task<bool> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent)
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

        public async Task<bool> RunDbMigrationScriptAsync(SqlDataBaseInfo sqlDataBaseInfo
            , Script script
            , DateTime executedDateTime)
        {
            SqlTransaction? transaction = null;
            try
            {
                string sqlFormattedDate = executedDateTime.ToString("yyyy-MM-dd HH:mm:ss");//TODO find better way than this way
                string updateVersioningTableScript = $@" 
                            USE {sqlDataBaseInfo.DatabaseName} 
                            INSERT INTO DbMigrationsRun (Executed, ScriptName, ScriptContent, version)
                            VALUES ('{sqlFormattedDate}', '{script.NamePart}', 'xx', '1.0.0');
                        ";

                using SqlConnection connection = new SqlConnection(sqlDataBaseInfo.ConnectionString);
                await connection.OpenAsync();

                transaction = connection.BeginTransaction(IsolationLevel.Serializable
                    , "EasyDbMigrator");

                using SqlCommand cmdScript = new(script.Content, connection, transaction);
                using SqlCommand cmdUpdateVersioningTable = new(updateVersioningTableScript, connection, transaction);

                _ = await cmdScript.ExecuteNonQueryAsync();
                _ = await cmdUpdateVersioningTable.ExecuteNonQueryAsync();

                transaction.Commit();

                return true;
            }
            catch (SqlException)//TODO return something useful and log this stuff
            {
                transaction?.Rollback();
                return false;
            }
        }
    }
}
