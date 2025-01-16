// Ignore Spelling: databasename
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
public class MigrationConfigurationTests
{
    [Theory]
    [InlineData(data: new object[] { "word", false })]
    [InlineData(data: new object[] { "word1 word2", true })]
    [InlineData(data: new object[] { "", true })]
    [InlineData(data: new object[] { " ", true })]
    public void TheParameterDatabasenameShouldHaveOnlyOneWord(string databaseName, bool shouldThrowException)
    {
        static void act(string databaseName)
        {
            MigrationConfiguration unused = new("connection string"
                , databaseName);
        }

        if (shouldThrowException)
        {
            Assert.Throws<ArgumentException>(() => act(databaseName));
        }
        else
        {
            act(databaseName);
        }
    }

    [Theory]
    [InlineData(new object[] { "", true })]
    public void TheParameterConnectionStringShouldBeCorrect(string connectionString, bool shouldThrowException)
    {
        static void act(string connectionString)
        {
            MigrationConfiguration unused = new(connectionString
                , "databaseName");
        }

        if (shouldThrowException)
        {
            Assert.Throws<ArgumentException>(() => act(connectionString));
        }
        else
        {
            act(connectionString);
        }
    }
}
