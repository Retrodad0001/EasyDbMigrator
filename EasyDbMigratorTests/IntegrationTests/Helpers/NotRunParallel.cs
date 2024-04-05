#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EasyDbMigratorTests.IntegrationTests.Helpers;

[ExcludeFromCodeCoverage]
[CollectionDefinition(nameof(NotRunParallel), DisableParallelization = true)]
public sealed class NotRunParallel
{
}
