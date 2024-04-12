using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace EasyDbMigrator.IntegrationTestHelpers;

/// <summary>
/// A logger provider for xunit tests.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly LoggerExternalScopeProvider _scopeProvider = new();


    /// <summary>
    /// Constructor for the logger provider.
    /// </summary>
    /// <param name="testOutputHelper"></param>
    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper) : base()
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// Creates a logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogHelper(_testOutputHelper, _scopeProvider, categoryName);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public void Dispose()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        //nothing to dispose here
    }
}