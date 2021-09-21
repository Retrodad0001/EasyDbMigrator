using EasyDbMigrator.Helpers;
using EasyDbMigrator.Infra;
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
            _ = result[0].FileName.Should().Be("20212230_001_CreateDB.sql");
            _ = result[0].DatePartOfName.Should().Be(new System.DateTime(2021, 12, 30));

            _ = result[1].SequenceNumberPart.Should().Be(2);
            _ = result[1].FileName.Should().Be("20212230_002_Script2.sql");
            _ = result[1].DatePartOfName.Should().Be(new System.DateTime(2021,12,30));

            _ = result[2].SequenceNumberPart.Should().Be(1);
            _ = result[2].FileName.Should().Be("20212231_001_Script1.sql");
            _ = result[2].DatePartOfName.Should().Be(new System.DateTime(2021, 12, 31));
        }
    }
}
