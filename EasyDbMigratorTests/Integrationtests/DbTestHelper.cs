using Dapper;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
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

        public bool CheckMigrationsTable(List<VersioningTableRow> expected, string testdbName)
        {
            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                connection.Open();

                List<VersioningTableRow> actual = (List<VersioningTableRow>)connection.Query<VersioningTableRow>(@$"
                    use {testdbName}
                    SELECT Id, Executed, ScriptName, ScriptContent, Version 
                    FROM DbMigrationsRun");

                _ = actual.Should().HaveSameCount(expected);
                _ = actual.Should().Contain(expected);

                return true;
            }
        }

        public async Task<bool> TryExecuteSQLScriptAsync(string scriptContent)
        {
            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                throw new ArgumentException("scriptContent cannot be empty, is there something wrong?");
            }

            using SqlConnection connection = new SqlConnection(connectionstring);
            using SqlCommand command = new(scriptContent, connection);

            await command.Connection.OpenAsync();
            _ = await command.ExecuteNonQueryAsync();

            return true;
        }
    }
}
