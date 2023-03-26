using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit.Abstractions;

namespace EasyDbMigrator.IntegrationTestHelpers;

[ExcludeFromCodeCoverage]
#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
public sealed class XUnitLogHelper<T> : XUnitLogHelper, ILogger<T>
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
{
    public XUnitLogHelper(ITestOutputHelper testOutputHelper
        , LoggerExternalScopeProvider scopeProvider)
        : base(testOutputHelper: testOutputHelper, scopeProvider: scopeProvider, categoryName: typeof(T).FullName)
    {
    }
}

[ExcludeFromCodeCoverage]
public class XUnitLogHelper : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string? _categoryName;
    private readonly LoggerExternalScopeProvider _scopeProvider;

    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper)
    {
        return new XUnitLogHelper(testOutputHelper: testOutputHelper, scopeProvider: new LoggerExternalScopeProvider(), categoryName: string.Empty);
    }

    public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper)
    {
        return new XUnitLogHelper<T>(testOutputHelper: testOutputHelper, scopeProvider: new LoggerExternalScopeProvider());
    }

    public XUnitLogHelper(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string? categoryName)
    {
        if (string.IsNullOrEmpty(value: categoryName))
        {
            throw new ArgumentException(message: $"'{nameof(categoryName)}' cannot be null or empty.", paramName: nameof(categoryName));
        }

        _categoryName = categoryName;
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(paramName: nameof(testOutputHelper));
        _scopeProvider = scopeProvider ?? throw new ArgumentNullException(paramName: nameof(scopeProvider));
    }
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }
#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
    public IDisposable BeginScope<TState>(TState state)
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
    {
        return _scopeProvider.Push(state: state);
    }
    public void Log<TState>(LogLevel logLevel
        , EventId eventId
        , TState state
        , Exception? exception
        , Func<TState, Exception?, string> formatter)
    {
        var sb = new StringBuilder();
        _ = sb.Append(value: GetLogLevelString(logLevel: logLevel))
          .Append(value: " [").Append(value: _categoryName).Append(value: "] ")
          .Append(value: formatter(arg1: state, arg2: exception));

        if (exception is not null)
        {
            _ = sb.Append(value: '\n').Append(handler: $"message : {exception.Message} , trace : {exception.StackTrace} ");
        }

        _scopeProvider.ForEachScope(callback: (scope, builder) =>
        {
            _ = builder.Append(value: "\n => ");
            _ = builder.Append(value: scope);
        }, state: sb);

        _testOutputHelper.WriteLine(message: sb.ToString());
    }
    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warning",
            LogLevel.Error => "Error",
            LogLevel.Critical => "Critical",
            LogLevel.None => throw new Exception(message: "this is a problem"),
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(logLevel))
        };
    }
}