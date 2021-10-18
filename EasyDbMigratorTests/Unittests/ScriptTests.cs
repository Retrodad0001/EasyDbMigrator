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
        public void when_creating_the_parameter_scriptname_should_be_correct(string filename, bool shouldThrowException)
        {
            Action act = () =>
            {
                Script sut = new Script(filename: filename, content: "xx");
            };

            if (shouldThrowException)
                _ = act.Should().Throw<ArgumentException>();
            else
                _ = act.Should().NotThrow();
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        public void when_creating_the_parameter_connectionsring_should_be_correct(string content, bool shouldThrowException)
        {
            Action act = () =>
            {
                Script sut = new(filename: "xx", content: content);
            };

            if (shouldThrowException)
                _ = act.Should().Throw<ArgumentException>();
            else
                _ = act.Should().NotThrow();
        }
    }
}
