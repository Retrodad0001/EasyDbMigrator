using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit.Abstractions;

namespace EasyDbMigrator.IntegrationTestHelpers
{
    [ExcludeFromCodeCoverage]//TODO FIX WHEN NO SUPPORT .NET 3.1
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public sealed class XUnitLoghelper<T> : XUnitLoghelper, ILogger<T>
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        public XUnitLoghelper(ITestOutputHelper testOutputHelper
            , LoggerExternalScopeProvider scopeProvider)
#pragma warning disable CS8604 // Possible null reference argument. //TODO FIX WHEN NO SUPPORT .NET 3.1
            : base(testOutputHelper, scopeProvider, typeof(T).FullName)
#pragma warning restore CS8604 // Possible null reference argument.
        {
        }
    }

    [ExcludeFromCodeCoverage]
    public class XUnitLoghelper : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;
        private readonly LoggerExternalScopeProvider _scopeProvider;

        public static ILogger CreateLogger(ITestOutputHelper testOutputHelper)
        {
            return new XUnitLoghelper(testOutputHelper, new LoggerExternalScopeProvider(), "");
        }

        public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper)
        {
            return new XUnitLoghelper<T>(testOutputHelper, new LoggerExternalScopeProvider());
        }

        public XUnitLoghelper(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string categoryName)
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
        }//TODO FIX WHEN NO SUPPORT .NET 3.1
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public void Log<TState>(LogLevel logLevel
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
            , EventId eventId
            , TState state
            , Exception exception
            , Func<TState, Exception, string> formatter)
        {
            var sb = new StringBuilder();
            _ = sb.Append(GetLogLevelString(logLevel))
              .Append(" [").Append(_categoryName).Append("] ")
              .Append(formatter(state, exception));

            if (exception != null)
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