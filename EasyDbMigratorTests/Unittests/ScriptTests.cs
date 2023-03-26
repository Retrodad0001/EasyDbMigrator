using EasyDbMigrator;
using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
public class ScriptTests
{
    [Theory]
    [InlineData(data: new object[] { "", true })]
    [InlineData(data: new object[] { " ", true })]
    public void When_creating_the_parameter_scriptName_should_be_correct(string filename, bool shouldThrowException)
    {
        var act = () =>
        {
            Script unused = new(filename: filename, content: "xx");
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
    public void When_creating_the_parameter_connectionString_should_be_correct(string content, bool shouldThrowException)
    {
        var act = () =>
        {
            Script unused = new(filename: "xx", content: content);
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