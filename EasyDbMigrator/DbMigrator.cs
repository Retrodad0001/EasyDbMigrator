using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TestLib;

namespace EasyDbMigrator
{
    public class DbMigrator
    {
        private readonly string _connectionstring;

        public DbMigrator(string connectionstring)
        {
            if (string.IsNullOrWhiteSpace(connectionstring))
            {
                throw new ArgumentException($"'{nameof(connectionstring)}' cannot be null or whitespace.", nameof(connectionstring));
            }

            _connectionstring = connectionstring;
        }

        public async Task<bool> TryApplyMigrationsAsync()
        {

            //always use master first

            bool succeeded1 = await TryExcecuteScriptAsync("ignore", @"use master");

            Assembly? assembly = Assembly.GetAssembly(typeof(Migration));

            if (assembly == null)
            {
                return false;//TODO return more specific error
            }

            var resourcenames = assembly.GetManifestResourceNames(); //TODO add test when there other resources than .sql files

            foreach (string resourcename in resourcenames)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourcename))
                {

                    if (stream is null)
                    {
                        return false;//TODO more specific result
                    }

                    using (StreamReader reader = new(stream))
                    {
                        string sqlScript = reader.ReadToEnd();
                        bool succeeded2 = await TryExcecuteScriptAsync(resourcename: resourcename, sqlScript: sqlScript);
                    }
                }
            }

            return true;
        }

        private async Task<bool> TryExcecuteScriptAsync(string resourcename, string sqlScript)
        {
            if (string.IsNullOrWhiteSpace(sqlScript))
            {
                throw new ArgumentException($"{resourcename} script is empty, is there something wrong?");
            }

            using SqlConnection connection = new SqlConnection(_connectionstring);
            using SqlCommand command = new(sqlScript, connection);

            await command.Connection.OpenAsync();
            _ = await command.ExecuteNonQueryAsync();

            return true;
        }
    }
}
