// Ignore Spelling: databasename

using EasyDbMigrator;
using FluentAssertions;
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
    public void The_parameter_databasename_should_have_only_one_word(string databaseName, bool shouldThrowException)
    {
        var act = () =>
        {
            MigrationConfiguration unused = new(connectionString: "connection string"
                , databaseName: databaseName);
        };

        if (shouldThrowException)
        {
            _ = act.Should().Throw<ArgumentException>();
        }
        else
        {
            _ = act.Should().NotThrow();
        }
    }

    [Theory]
    [InlineData(data: new object[] { "", true })]
    [InlineData(data: new object[] { " ", true })]
    public void The_parameter_connectionString_should_be_correct(string connectionString, bool shouldThrowException)
    {
        var act = () =>
        {
            MigrationConfiguration unused = new(connectionString: connectionString
                , databaseName: "databaseName");
        };

        if (shouldThrowException)
        {
            _ = act.Should().Throw<ArgumentException>();
        }
        else
        {
            _ = act.Should().NotThrow();
        }
    }
}
