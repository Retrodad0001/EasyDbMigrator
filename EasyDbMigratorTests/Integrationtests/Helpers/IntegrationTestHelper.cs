using Dapper;
using EasyDbMigratorTests.TestHelpers;
using FluentAssertions;
using Npgsql;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.Integrationtests.Helpers
{
    [ExcludeFromCodeCoverage]
    public sealed class IntegrationTestHelper
    {
        public static bool CheckMigrationsTableSqlSever(string connectionString,
            List<DbMigrationsRunRowSqlServer> expectedRows
            , string testDatabaseName)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            List<DbMigrationsRunRowSqlServer> actual = (List<DbMigrationsRunRowSqlServer>)connection.Query<DbMigrationsRunRowSqlServer>(@$"
                    use {testDatabaseName}
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun");

            _ = actual.Should().HaveSameCount(expectedRows);
            _ = actual.Should().Contain(expectedRows);

            return true;
        }

        public static bool CheckMigrationsTablePostgresSever(string connectionString,
           List<DbMigrationsRunRowPostgressServer> expectedRows)
        {
            using NpgsqlConnection connection = new(connectionString);
            connection.Open();

            List<DbMigrationsRunRowPostgressServer> actual = (List<DbMigrationsRunRowPostgressServer>)connection.Query<DbMigrationsRunRowPostgressServer>(@$"
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun");

            _ = actual.Should().HaveSameCount(expectedRows);
            _ = actual.Should().Contain(expectedRows);

            return true;
        }
    }
}
