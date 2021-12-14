using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit.Abstractions;

namespace EasyDbMigrator.IntegrationTestHelpers
{
    [ExcludeFromCodeCoverage]
    public sealed class XUnitLoghelper<T> : XUnitLoghelper, ILogger<T>
    {
        public XUnitLoghelper(ITestOutputHelper testOutputHelper
            , LoggerExternalScopeProvider scopeProvider)
            : base(testOutputHelper, scopeProvider, typeof(T).FullName)
        {
        }
    }

    [ExcludeFromCodeCoverage]
    public class XUnitLoghelper : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string? _categoryName;
        private readonly LoggerExternalScopeProvider _scopeProvider;

        public static ILogger CreateLogger(ITestOutputHelper testOutputHelper)
        {
            return new XUnitLoghelper(testOutputHelper, new LoggerExternalScopeProvider(), string.Empty);
        }

        public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper)
        {
            return new XUnitLoghelper<T>(testOutputHelper, new LoggerExternalScopeProvider());
        }

        public XUnitLoghelper(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string? categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentException($"'{nameof(categoryName)}' cannot be null or empty.", nameof(categoryName));
            }

            _categoryName = categoryName;
            _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return _scopeProvider.Push(state);
        }
        public void Log<TState>(LogLevel logLevel
            , EventId eventId
            , TState state
            , Exception? exception
            , Func<TState, Exception?, string> formatter)
        {
            var sb = new StringBuilder();
            _ = sb.Append(GetLogLevelString(logLevel))
              .Append(" [").Append(_categoryName).Append("] ")
              .Append(formatter(state, exception));

            if (exception is not null)
            {
                _ = sb.Append('\n').Append($"message : {exception.Message} , trace : {exception.StackTrace} ");
            }

            _scopeProvider.ForEachScope((scope, state) =>
            {
                _ = state.Append("\n => ");
                _ = state.Append(scope);
            }, sb);

            _testOutputHelper.WriteLine(sb.ToString());
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
                LogLevel.None => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }
    }
}