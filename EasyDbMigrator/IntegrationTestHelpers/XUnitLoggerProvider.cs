using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace EasyDbMigrator.IntegrationTestHelpers;

[ExcludeFromCodeCoverage]
public sealed class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly LoggerExternalScopeProvider _scopeProvider = new();

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogHelper(testOutputHelper: _testOutputHelper, scopeProvider: _scopeProvider, categoryName: categoryName);
    }

    public void Dispose()
    {
        //nothing to dispose here
    }
}