// Ignore Spelling: Postgres Sql

using Dapper;
using EasyDbMigratorTests.TestHelpers;
using FluentAssertions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.IntegrationTests.Helpers;

[ExcludeFromCodeCoverage]
public static class IntegrationTestHelper
{
    public static bool CheckMigrationsTableSqlSever(string connectionString,
        List<DbMigrationsRunRowSqlServer> expectedRows
        , string testDatabaseName)
    {
        using SqlConnection connection = new(connectionString: connectionString);
        connection.Open();

        var actual = (List<DbMigrationsRunRowSqlServer>)connection.Query<DbMigrationsRunRowSqlServer>(sql: @$"
                    use {testDatabaseName}
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun");

        _ = actual.Should().HaveSameCount(otherCollection: expectedRows);
        _ = actual.Should().Contain(expected: expectedRows);

        return true;
    }

    public static bool CheckMigrationsTablePostgresSever(string connectionString,
       List<DbMigrationsRunRowPostgresServer> expectedRows)
    {
        using NpgsqlConnection connection = new(connectionString: connectionString);
        connection.Open();

        var actual = (List<DbMigrationsRunRowPostgresServer>)connection.Query<DbMigrationsRunRowPostgresServer>(sql: @$"
                    SELECT Id, Executed, Filename, Version 
                    FROM DbMigrationsRun");

        _ = actual.Should().HaveSameCount(otherCollection: expectedRows);
        _ = actual.Should().Contain(expected: expectedRows);

        return true;
    }
    
    public static string GenerateRandomDatabaseName()
    {
   //     string result = Guid.NewGuid().ToString().Replace("-", "");
        string result = "test" + new Random().Next(minValue: 100000, maxValue: 999999) ;
        return result;
    }
}
