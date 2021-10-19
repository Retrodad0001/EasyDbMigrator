using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Integrationtests.Helpers
{
    [ExcludeFromCodeCoverage]
    [CollectionDefinition(nameof(NotRunParallel), DisableParallelization = true)]
    public class NotRunParallel
    {
    }
}
