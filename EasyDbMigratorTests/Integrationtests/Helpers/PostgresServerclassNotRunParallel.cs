using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Integrationtests.Helpers
{
    [ExcludeFromCodeCoverage]
    [CollectionDefinition(nameof(PostgresServerclassNotRunParallel), DisableParallelization = false)]
    public class PostgresServerclassNotRunParallel
    {
    }
}
