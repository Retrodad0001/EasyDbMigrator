#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
[TestClass]
public class ScriptTests
{
    [TestMethod]
    [DataRow("", true)]
    [DataRow(" ", true)]
    public void WhenCreatingTheParameterScriptNameShouldBeCorrect(string filename, bool shouldThrowException)
    {
        static void act(string filename)
        {
            Script unused = new(filename, "xx");
        }

        if (shouldThrowException)
        {
            Assert.ThrowsExactly<ArgumentException>(() => act(filename));
        }
        else
        {
            act(filename);
        }
    }

    [TestMethod]
    [DataRow("", true)]
    [DataRow(" ", true)]
    public void WhenCreatingTheParameterConnectionStringShouldBeCorrect(string content, bool shouldThrowException)
    {
        static void act(string content)
        {
            Script unused = new("xx", content);
        }

        if (shouldThrowException)
        {
            Assert.ThrowsExactly<ArgumentException>(() => act(content));
        }
        else
        {
            act(content);
        }
    }
}
