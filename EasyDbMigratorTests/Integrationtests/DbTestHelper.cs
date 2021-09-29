using Dapper;
using FluentAssertions;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage] //this class is used for testing only
    public class DbTestHelper
    {
        public bool CheckMigrationsTable(string connectionString,
            List<DbMigrationsRunRow> expectedRows
            , string testdbName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<DbMigrationsRunRow> actual = (List<DbMigrationsRunRow>)connection.Query<DbMigrationsRunRow>(@$"
                    use {testdbName}
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun");

                _ = actual.Should().HaveSameCount(expectedRows);
                _ = actual.Should().Contain(expectedRows);

                return true;
            }
        }
    }
}
