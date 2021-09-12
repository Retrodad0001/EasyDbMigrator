using EasyDbMigrator.Helpers;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TestLib;
using Xunit;

namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class ScriptHelperTests
    {
        [Fact]
        public async Task TryConvertResourceNamesToScriptsInCorrectSequenceByTypeAsync()
        {
            var sut = new ScriptsHelper();

            var result = await sut.TryConvertoScriptsInCorrectSequenceByTypeAsync(typeof(SomeCustomClass));

            _ = result.Should().HaveCount(3);

            _ = result[0].SequenceNumberPart.Should().Be(1);
            _ = result[0].NamePart.Should().Be("CreateDB");
            _ = result[0].DatePartOfName.Should().Be(new System.DateTime(2021, 12, 30));

            _ = result[1].SequenceNumberPart.Should().Be(2);
            _ = result[1].NamePart.Should().Be("Script2");
            _ = result[1].DatePartOfName.Should().Be(new System.DateTime(2021,12,30));

            _ = result[2].SequenceNumberPart.Should().Be(1);
            _ = result[2].NamePart.Should().Be("Script1");
            _ = result[2].DatePartOfName.Should().Be(new System.DateTime(2021, 12, 31));

        }
    }
}
