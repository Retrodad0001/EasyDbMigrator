using Dapper;
using FluentAssertions;
using Npgsql;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
    public class IntegrationTestHelper
    {
        public bool CheckMigrationsTableSqlSever(string connectionString,
            List<DbMigrationsRunRowSqlServer> expectedRows
            , string testDatabaseName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<DbMigrationsRunRowSqlServer> actual = (List<DbMigrationsRunRowSqlServer>)connection.Query<DbMigrationsRunRowSqlServer>(@$"
                    use {testDatabaseName}
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun");

                _ = actual.Should().HaveSameCount(expectedRows);
                _ = actual.Should().Contain(expectedRows);

                return true;
            }
        }

        public bool CheckMigrationsTablePostgresSever(string connectionString,
           List<DbMigrationsRunTest> expectedRows)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                List<DbMigrationsRunTest> actual = (List<DbMigrationsRunTest>)connection.Query<DbMigrationsRunTest>(@$"
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun");

                _ = actual.Should().HaveSameCount(expectedRows);
                _ = actual.Should().Contain(expectedRows);

                return true;
            }
        }

        //TODO update wiki
    }
}
