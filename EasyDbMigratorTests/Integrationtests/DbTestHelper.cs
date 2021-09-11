using System;
using System.Data.SqlClient;

namespace EasyDbMigratorTests.Integrationtests
{
    public class DbTestHelper
    {
        private readonly string connectionstring;

        public DbTestHelper(string connectionstring)
        {
            if (string.IsNullOrEmpty(connectionstring))
            {
                throw new ArgumentException($"'{nameof(connectionstring)}' cannot be null or empty.", nameof(connectionstring));
            }

            this.connectionstring = connectionstring;
        }

        public bool CheckMigrationsTable()
        {
            return false;
        }

        private bool TryExecuteSQLScript(string resourcename, string scriptContent)
        {
            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                throw new ArgumentException($"{resourcename} cannot be empty, is there something wrong?");
            }

            using SqlConnection connection = new SqlConnection(connectionstring);
            using SqlCommand command = new(scriptContent, connection);

                command.Connection.Open();
            _ = command.ExecuteNonQuery();

            return true;
        }
    }
}
