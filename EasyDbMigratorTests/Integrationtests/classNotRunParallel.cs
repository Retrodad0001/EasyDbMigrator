using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
    [CollectionDefinition(nameof(classNotRunParallel), DisableParallelization = true)]
    public class classNotRunParallel 
    {
    }
}
