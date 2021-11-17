using EasyDbMigrator;
using ExampleTestLibWithSqlServerScripts;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class AssemblyResourceHelperTests
    {
        [Fact]
        public async Task TryConvertResourceNamesToScriptsInCorrectSequenceByTypeAsync()
        {
            var sut = new AssemblyResourceHelper();

            var result = await sut.TryGetScriptsFromAssembly(typeof(HereTheSQLServerScriptsCanBeFound)).ConfigureAwait(true);

            _ = result.Should().HaveCount(3);
            _ = result.TrueForAll(script => script.FileName != string.Empty);
            _ = result.TrueForAll(script => script.FileName.Split('.').Length == 2);//contain only one
        }
    }
}
