using Dapper;
using FluentAssertions;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.Integrationtests
{
    //TODO *** use https://codecov.io and put link in readme.md
    [ExcludeFromCodeCoverage]
    public class DbTestHelper
    {

        public DbTestHelper()
        { 
        }

        public bool CheckMigrationsTable(string connectionString,
            List<VersioningTableRow> expectedRows
            , string testdbName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<VersioningTableRow> actual = (List<VersioningTableRow>)connection.Query<VersioningTableRow>(@$"
                    use {testdbName}
                    SELECT Id, Executed, ScriptName, Version 
                    FROM DbMigrationsRun");

                _ = actual.Should().HaveSameCount(expectedRows);
                _ = actual.Should().Contain(expectedRows);

                return true;
            }
        }
    }
}
