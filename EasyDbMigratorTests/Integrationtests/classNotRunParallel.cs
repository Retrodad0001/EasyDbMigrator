using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
    [CollectionDefinition(nameof(PostgresServerclassNotRunParallel), DisableParallelization = true)]
    public class PostgresServerclassNotRunParallel
    {
    }

    [ExcludeFromCodeCoverage]
    [CollectionDefinition(nameof(SqlServerclassNotRunParallel), DisableParallelization = true)]
    public class SqlServerclassNotRunParallel
    {
    }
}
