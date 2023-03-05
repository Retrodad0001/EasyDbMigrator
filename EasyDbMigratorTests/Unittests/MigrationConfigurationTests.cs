using EasyDbMigrator;
using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class MigrationConfigurationTests
    {
        [Theory]
        [InlineData("word", false)]
        [InlineData("word1 word2", true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        public void The_parameter_databasename_should_have_only_one_word(string databasename, bool shouldThrowException)
        {
            var act = () =>
            {
                MigrationConfiguration sut = new("connection string"
                    , databasename);
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
        public void The_parameter_connectionString_should_be_correct(string connectionString, bool shouldThrowException)
        {
            var act = () =>
            {
                MigrationConfiguration sut = new(connectionString
                    , "databasename");
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
