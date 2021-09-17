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
       // [InlineData("x.sql", false)] //TODO FIX ME
        public void the_parameter_scriptname_should_be_correct(string scriptname, bool shouldThrowException)
        {
            Action act = () =>
            {
                Script sut = new Script(scriptname: scriptname, content: "xx");
            };

            if (shouldThrowException)
                _ = act.Should().Throw<ArgumentException>();
            else
                _ = act.Should().NotThrow();
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        public void the_parameter_connectionsring_should_be_correct(string content, bool shouldThrowException)
        {
            Action act = () =>
            {
                Script sut = new Script(scriptname: "xx", content: content);
            };

            if (shouldThrowException)
                _ = act.Should().Throw<ArgumentException>();
            else
                _ = act.Should().NotThrow();
        }
    }
}
