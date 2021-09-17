using EasyDbMigrator;
using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class SqlDatabaseInfoTests
    {
        [Theory]
        [InlineData("xxxxxx" , false)]
        [InlineData("xxxxxx xxxxxx", true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        public void the_parameter_databasename_should_have_only_one_word(string databasename, bool shouldThrowException)
        {
            Action act = () =>
            {
                SqlDataBaseInfo sut = new SqlDataBaseInfo("connection string", databasename);
            };

            if (shouldThrowException)
                _ = act.Should().Throw<ArgumentException>();
            else
                _ = act.Should().NotThrow();
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        public void the_parameter_connectionsring_should_be_correct(string connentionsting, bool shouldThrowException)
        {
            Action act = () =>
            {
                SqlDataBaseInfo sut = new SqlDataBaseInfo(connentionsting, "databasename");
            };

            if (shouldThrowException)
                _ = act.Should().Throw<ArgumentException>();
            else
                _ = act.Should().NotThrow();
        }
    }
}
