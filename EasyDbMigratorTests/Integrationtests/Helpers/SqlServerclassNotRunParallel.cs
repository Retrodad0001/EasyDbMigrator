using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Integrationtests.Helpers
{
    [ExcludeFromCodeCoverage]
    [CollectionDefinition(nameof(SqlServerclassNotRunParallel), DisableParallelization = false)]
    public class SqlServerclassNotRunParallel
    {
    }
}
