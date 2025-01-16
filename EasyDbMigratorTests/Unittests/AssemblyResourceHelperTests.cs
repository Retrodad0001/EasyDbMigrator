#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using ExampleTestLibWithSqlServerScripts;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
public class AssemblyResourceHelperTests
{
    [Fact]
    public async Task TryConvertResourceNamesToScriptsInCorrectSequenceByTypeAsync()
    {
        AssemblyResourceHelper? sut = new();

        var result = await sut.TryGetScriptsFromAssembly(typeof(HereTheSqlServerScriptsCanBeFound));

        Assert.Equal(expected: 3, actual: result.Count);

        _ = result.TrueForAll(script => script.FileName != string.Empty);
        _ = result.TrueForAll(script => script.FileName.Split('.').Length == 2);//contain only one
    }
}
