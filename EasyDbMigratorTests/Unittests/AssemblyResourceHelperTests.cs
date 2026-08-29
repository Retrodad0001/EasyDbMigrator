#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using ExampleTestLibWithSqlServerScripts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
[TestClass]
public class AssemblyResourceHelperTests
{
    [TestMethod]
    public async Task TryConvertResourceNamesToScriptsInCorrectSequenceByTypeAsync()
    {
        AssemblyResourceHelper? sut = new();

        var result = await sut.TryGetScriptsFromAssembly(typeof(HereTheSqlServerScriptsCanBeFound));

        Assert.HasCount(expected: 3, collection: result);

        _ = result.TrueForAll(script => script.FileName != string.Empty);
        _ = result.TrueForAll(script => script.FileName.Split('.').Length == 2);
    }

    [TestMethod]
    public async Task TryGetScriptsFromAssemblyThrowsWhenTypeIsNull()
    {
        AssemblyResourceHelper sut = new();

        _ = await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => sut.TryGetScriptsFromAssembly(null!));
    }
}
