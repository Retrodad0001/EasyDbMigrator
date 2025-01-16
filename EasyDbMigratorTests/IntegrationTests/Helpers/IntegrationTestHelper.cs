// Ignore Spelling: Postgres Sql
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Dapper;
using EasyDbMigratorTests.TestHelpers;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace EasyDbMigratorTests.IntegrationTests.Helpers;

[ExcludeFromCodeCoverage]
public static class IntegrationTestHelper
{
    public static bool CheckMigrationsTableSqlSever(string connectionString,
        List<DbMigrationsRunRowSqlServer> expectedRows
        , string testDatabaseName)
    {
        using SqlConnection connection = new(connectionString);
        connection.Open();

        var actual = (List<DbMigrationsRunRowSqlServer>)connection.Query<DbMigrationsRunRowSqlServer>(
            new StringBuilder().Append(@"
                    use ")
                .Append(testDatabaseName)
                .Append(@"
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun")
                .ToString());

        Assert.Equal(expected: expectedRows.Count, actual: actual.Count);

        for (int i = 0; i < actual.Count; i++)
        {
            Assert.Equal(expectedRows[i].Id, actual[i].Id);
            Assert.Equal(expectedRows[i].Executed, actual[i].Executed);
            Assert.Equal(expectedRows[i].Filename, actual[i].Filename);
            Assert.Equal(expectedRows[i].Version, actual[i].Version);
        }

        return true;
    }

    public static bool CheckMigrationsTablePostgresSever(string connectionString,
           List<DbMigrationsRunRowPostgresServer> expectedRows)
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();

        var actual = (List<DbMigrationsRunRowPostgresServer>)connection.Query<DbMigrationsRunRowPostgresServer>(
            new StringBuilder().Append(@"
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun")
                .ToString());

        Assert.Equal(expected: expectedRows.Count, actual: actual.Count);

        for (int i = 0; i < actual.Count; i++)
        {
            Assert.Equal(expectedRows[i].Id, actual[i].Id);
            Assert.Equal(expectedRows[i].Executed, actual[i].Executed);
            Assert.Equal(expectedRows[i].Filename, actual[i].Filename);
            Assert.Equal(expectedRows[i].Version, actual[i].Version);
        }

        return true;
    }

    public static string GenerateRandomDatabaseName()
    {
        string result = new StringBuilder().Append("test").Append(new Random().Next(100000, 999999)).ToString();
        return result;
    }
}
