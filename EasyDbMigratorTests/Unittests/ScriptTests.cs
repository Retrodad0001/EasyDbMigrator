#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
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
    public void WhenCreatingTheParameterScriptNameShouldBeCorrect(string filename, bool shouldThrowException)
    {
        static void act(string filename)
        {
            Script unused = new(filename, "xx");
        }

        if (shouldThrowException)
        {
            Assert.Throws<ArgumentException>(() => act(filename));
        }
        else
        {
            act(filename);
        }
    }

    [Theory]
    [InlineData(data: new object[] { "", true })]
    [InlineData(data: new object[] { " ", true })]
    public void WhenCreatingTheParameterConnectionStringShouldBeCorrect(string content, bool shouldThrowException)
    {
        static void act(string content)
        {
            Script unused = new("xx", content);
        }

        if (shouldThrowException)
        {
            Assert.Throws<ArgumentException>(() => act(content));
        }
        else
        {
            act(content);
        }
    }
}