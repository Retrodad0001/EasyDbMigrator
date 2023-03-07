using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.IntegrationTests.Helpers;

[ExcludeFromCodeCoverage]
[CollectionDefinition(nameof(NotRunParallel), DisableParallelization = true)]
public sealed class NotRunParallel
{
}
