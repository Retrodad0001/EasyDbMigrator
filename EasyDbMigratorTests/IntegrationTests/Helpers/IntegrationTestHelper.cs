// Ignore Spelling: Postgres Sql
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Dapper;
using EasyDbMigratorTests.TestHelpers;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace EasyDbMigratorTests.IntegrationTests.Helpers;

[ExcludeFromCodeCoverage]
public static class IntegrationTestHelper
{
    public static List<DbMigrationsRunRowSqlServer> ReadMigrationsRunTableSqlServer(string connectionString
        , string testDatabaseName)
    {
        using SqlConnection connection = new(connectionString);
        connection.Open();

        return (List<DbMigrationsRunRowSqlServer>)connection.Query<DbMigrationsRunRowSqlServer>(
            new StringBuilder().Append(@"
                    use ")
                .Append(testDatabaseName)
                .Append(@"
                    SELECT Id, Executed, Filename, Version
                    FROM DbMigrationsRun")
                .ToString());
    }

    public static bool CheckMigrationsTableSqlSever(string connectionString,
        List<DbMigrationsRunRowSqlServer> expectedRows
        , string testDatabaseName)
    {
        List<DbMigrationsRunRowSqlServer> actual = ReadMigrationsRunTableSqlServer(connectionString, testDatabaseName);

        string context = new StringBuilder()
            .Append("database: ").Append(testDatabaseName).Append(Environment.NewLine)
            .Append("expected DbMigrationsRun rows:").Append(Environment.NewLine)
            .Append(DescribeRows(expectedRows))
            .Append("actual DbMigrationsRun rows:").Append(Environment.NewLine)
            .Append(DescribeRows(actual))
            .ToString();

        Assert.HasCount(expected: expectedRows.Count, collection: actual
            , message: new StringBuilder().Append("wrong number of rows in DbMigrationsRun").Append(Environment.NewLine).Append(context).ToString());

        for (int i = 0; i < actual.Count; i++)
        {
            AssertRowValue(i, "Id", expectedRows[i].Id, actual[i].Id, context);
            AssertRowValue(i, "Executed", expectedRows[i].Executed, actual[i].Executed, context);
            AssertRowValue(i, "Filename", expectedRows[i].Filename, actual[i].Filename, context);
            AssertRowValue(i, "Version", expectedRows[i].Version, actual[i].Version, context);
        }

        return true;
    }

    public static List<DbMigrationsRunRowPostgresServer> ReadMigrationsRunTablePostgres(string connectionString)
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();

        return (List<DbMigrationsRunRowPostgresServer>)connection.Query<DbMigrationsRunRowPostgresServer>(
            new StringBuilder().Append(@"
                    SELECT Id, Executed, Filename, Version
                    FROM DbMigrationsRun")
                .ToString());
    }

    public static bool CheckMigrationsTablePostgresSever(string connectionString,
           List<DbMigrationsRunRowPostgresServer> expectedRows)
    {
        List<DbMigrationsRunRowPostgresServer> actual = ReadMigrationsRunTablePostgres(connectionString);

        string context = new StringBuilder()
            .Append("expected DbMigrationsRun rows:").Append(Environment.NewLine)
            .Append(DescribeRows(expectedRows))
            .Append("actual DbMigrationsRun rows:").Append(Environment.NewLine)
            .Append(DescribeRows(actual))
            .ToString();

        Assert.HasCount(expected: expectedRows.Count, collection: actual
            , message: new StringBuilder().Append("wrong number of rows in DbMigrationsRun").Append(Environment.NewLine).Append(context).ToString());

        for (int i = 0; i < actual.Count; i++)
        {
            AssertRowValue(i, "Id", expectedRows[i].Id, actual[i].Id, context);
            AssertRowValue(i, "Executed", expectedRows[i].Executed, actual[i].Executed, context);
            AssertRowValue(i, "Filename", expectedRows[i].Filename, actual[i].Filename, context);
            AssertRowValue(i, "Version", expectedRows[i].Version, actual[i].Version, context);
        }

        return true;
    }

    public static bool TableExistsSqlServer(string connectionString, string databaseName, string tableName)
    {
        using SqlConnection connection = new(connectionString);
        connection.Open();

        int count = connection.ExecuteScalar<int>(new StringBuilder()
            .Append("USE ").Append(databaseName).Append(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName")
            .ToString(), new { tableName });

        return count > 0;
    }

    public static bool DatabaseExistsSqlServer(string connectionString, string databaseName)
    {
        SqlConnectionStringBuilder builder = new(connectionString)
        {
            InitialCatalog = "master"
        };

        using SqlConnection connection = new(builder.ConnectionString);
        connection.Open();

        int count = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM master.sys.databases WHERE name = @databaseName"
            , new { databaseName });

        return count > 0;
    }

    /// <summary>
    /// Lists the tables in the test database. Handy in a failure report to see
    /// how far the migration run got before it gave up.
    /// </summary>
    public static string DescribeTablesSqlServer(string connectionString, string databaseName)
    {
        try
        {
            if (!DatabaseExistsSqlServer(connectionString, databaseName))
            {
                return new StringBuilder().Append("database ").Append(databaseName).Append(" does not exist").ToString();
            }

            using SqlConnection connection = new(connectionString);
            connection.Open();

            var tables = connection.Query<string>(new StringBuilder()
                .Append("USE ").Append(databaseName).Append(@"
                    SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME")
                .ToString());

            return new StringBuilder().Append("tables in ").Append(databaseName).Append(": ")
                .Append(string.Join(", ", tables))
                .ToString();
        }
        catch (Exception ex)
        {
            return new StringBuilder().Append("could not list the tables in ").Append(databaseName)
                .Append(": ").Append(ex.Message).ToString();
        }
    }

    /// <summary>
    /// Postgres folds unquoted identifiers to lower case, so a table created as
    /// "Customers" is looked up here as "customers".
    /// </summary>
    public static bool TableExistsPostgres(string connectionString, string tableName)
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();

        int count = connection.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM pg_catalog.pg_tables
                WHERE schemaname = 'public' AND tablename = @tableName"
            , new { tableName = tableName.ToLowerInvariant() });

        return count > 0;
    }

    /// <summary>
    /// PostgreSqlConnector quotes the database identifier, so the name round-trips
    /// exactly and is matched here without any case folding.
    /// </summary>
    public static bool DatabaseExistsPostgres(string connectionString, string databaseName)
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();

        int count = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM pg_database WHERE datname = @databaseName"
            , new { databaseName });

        return count > 0;
    }

    /// <summary>
    /// Points an existing Postgres connection string at a specific database, the
    /// way PostgreSqlConnector does when it runs the migration scripts.
    /// </summary>
    public static string BuildPostgresConnectionStringForDatabase(string connectionString, string databaseName)
    {
        NpgsqlConnectionStringBuilder builder = new(connectionString)
        {
            Database = databaseName
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// Lists the tables in the public schema of the Postgres test database.
    /// Handy in a failure report to see how far the migration run got.
    /// </summary>
    public static string DescribeTablesPostgres(string connectionString)
    {
        try
        {
            using NpgsqlConnection connection = new(connectionString);
            connection.Open();

            var tables = connection.Query<string>(@"
                SELECT tablename FROM pg_catalog.pg_tables
                WHERE schemaname = 'public' ORDER BY tablename");

            return new StringBuilder().Append("tables in the public schema: ")
                .Append(string.Join(", ", tables))
                .ToString();
        }
        catch (Exception ex)
        {
            return new StringBuilder().Append("could not list the tables: ").Append(ex.Message).ToString();
        }
    }

    private static void AssertRowValue<T>(int rowIndex, string columnName, T expected, T actual, string context)
    {
        Assert.AreEqual(expected: expected, actual: actual
            , message: new StringBuilder()
                .Append("row ").Append(rowIndex.ToString(CultureInfo.InvariantCulture))
                .Append(" column ").Append(columnName).Append(" does not match")
                .Append(Environment.NewLine).Append(context)
                .ToString());
    }

    public static string DescribeRows<T>(List<T> rows)
    {
        if (rows.Count == 0)
        {
            return new StringBuilder().Append("    <no rows>").Append(Environment.NewLine).ToString();
        }

        StringBuilder builder = new();
        foreach (T row in rows)
        {
            _ = builder.Append("    ").Append(row).Append(Environment.NewLine);
        }

        return builder.ToString();
    }

    private static int _databaseNameCounter;

    public static string GenerateRandomDatabaseName()
    {
        int unique = Interlocked.Increment(ref _databaseNameCounter);
        string result = new StringBuilder().Append("test")
            .Append(Random.Shared.Next(100000, 999999))
            .Append(unique)
            .ToString();
        return result;
    }
}
