using EasyDbMigrator;
using ExampleTestLibWithSqlServerScripts;
using FluentAssertions;
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
        var sut = new AssemblyResourceHelper();

        var result = await sut.TryGetScriptsFromAssembly(typeOfClassWhereScriptsAreLocated: typeof(HereTheSqlServerScriptsCanBeFound)).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().HaveCount(expected: 3);
        _ = result.TrueForAll(match: script => script.FileName != string.Empty);
        _ = result.TrueForAll(match: script => script.FileName.Split(separator: '.').Length == 2);//contain only one
    }
}
