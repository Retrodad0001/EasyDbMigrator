using EasyDbMigrator.Helpers;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class ManifestResourceHelperTests
    {
        [Fact]
        public void when_type_is_found()
        {
            ManifestResourceHelper sut = new ManifestResourceHelper();

            string[] result = sut.TryGetListOfResourceNamesFromAssemblyByType(typeof(TestLib.SomeCustomClass));

            _ = result.Should().HaveCount(3);
        } 
    }
}
