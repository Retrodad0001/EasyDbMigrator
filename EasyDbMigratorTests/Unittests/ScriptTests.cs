#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
    [InlineData(new object[] { "", true })]
    [InlineData(new object[] { " ", true })]
    // ReSharper disable once HeapView.ClosureAllocation
    public void When_creating_the_parameter_scriptName_should_be_correct(string filename, bool shouldThrowException)
    {
        Action? act = () =>
        {
            Script unused = new(filename, "xx");
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
    [InlineData(new object[] { " ", true })]
    // ReSharper disable once HeapView.ClosureAllocation
    public void When_creating_the_parameter_connectionString_should_be_correct(string content, bool shouldThrowException)
    {
        Action? act = () =>
        {
            Script unused = new("xx", content);
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