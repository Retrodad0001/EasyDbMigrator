using EasyDbMigrator;
using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class ScriptTests
    {
        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        public void when_creating_the_parameter_scriptName_should_be_correct(string filename, bool shouldThrowException)
        {
            var act = () =>
            {
                // ReSharper disable once UnusedVariable
                Script sut = new(filename, "xx");
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
        [InlineData("", true)]
        [InlineData(" ", true)]
        public void when_creating_the_parameter_connectionString_should_be_correct(string content, bool shouldThrowException)
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            Action act = () =>
            {
                Script sut = new("xx", content);
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
}