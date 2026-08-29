// Ignore Spelling: databasename
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
[TestClass]
public class MigrationConfigurationTests
{
    [TestMethod]
    [DataRow("word", false)]
    [DataRow("word1 word2", true)]
    [DataRow("", true)]
    [DataRow(" ", true)]
    public void TheParameterDatabasenameShouldHaveOnlyOneWord(string databaseName, bool shouldThrowException)
    {
        static void act(string databaseName)
        {
            MigrationConfiguration unused = new("connection string"
                , databaseName);
        }

        if (shouldThrowException)
        {
            Assert.ThrowsExactly<ArgumentException>(() => act(databaseName));
        }
        else
        {
            act(databaseName);
        }
    }

    [TestMethod]
    [DataRow("", true)]
    public void TheParameterConnectionStringShouldBeCorrect(string connectionString, bool shouldThrowException)
    {
        static void act(string connectionString)
        {
            MigrationConfiguration unused = new(connectionString
                , "databaseName");
        }

        if (shouldThrowException)
        {
            Assert.ThrowsExactly<ArgumentException>(() => act(connectionString));
        }
        else
        {
            act(connectionString);
        }
    }
}
