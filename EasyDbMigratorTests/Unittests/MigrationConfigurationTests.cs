// Ignore Spelling: databasename
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
        Action? act = () =>
        {
            MigrationConfiguration unused = new("connection string"
                , databaseName);
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
    [InlineData(new object[] { "", true })]
    public void The_parameter_connectionString_should_be_correct(string connectionString, bool shouldThrowException)
    {
        Action? act = () =>
        {
            MigrationConfiguration unused = new(connectionString
                , "databaseName");
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
