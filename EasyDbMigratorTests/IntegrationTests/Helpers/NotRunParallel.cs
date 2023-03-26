using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.IntegrationTests.Helpers;

[ExcludeFromCodeCoverage]
[CollectionDefinition(name: nameof(NotRunParallel), DisableParallelization = true)]
public sealed class NotRunParallel
{
}
