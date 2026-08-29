#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDbMigratorTests.TestHelpers;

/// <summary>
/// A hand written ILogger of the kind a consumer of the package would supply
/// from their own integration test. It deliberately uses no mocking framework,
/// so it proves that EasyDbMigrator asks for nothing beyond ILogger itself.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ConsumerSuppliedLogger<T> : ILogger<T>
{
    private readonly List<ConsumerLogEntry> _entries = new();

    public IReadOnlyList<ConsumerLogEntry> Entries => _entries;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return null!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel
        , EventId eventId
        , TState state
        , Exception exception
        , Func<TState, Exception, string> formatter)
    {
        string message = formatter is null
            ? state?.ToString() ?? string.Empty
            : formatter(state, exception);

        _entries.Add(new ConsumerLogEntry(logLevel, message, exception));
    }

    public bool Received(LogLevel logLevel, string message)
    {
        foreach (ConsumerLogEntry entry in _entries)
        {
            if (entry.LogLevel == logLevel && string.Equals(entry.Message, message, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public string Describe()
    {
        if (_entries.Count == 0)
        {
            return "<the consumer logger received nothing>";
        }

        StringBuilder builder = new();
        foreach (ConsumerLogEntry entry in _entries)
        {
            _ = builder.Append(entry.LogLevel).Append(": ").Append(entry.Message);

            if (entry.Exception is not null)
            {
                _ = builder.Append(Environment.NewLine).Append("        exception: ").Append(entry.Exception);
            }

            _ = builder.Append(Environment.NewLine);
        }

        return builder.ToString();
    }
}

[ExcludeFromCodeCoverage]
public sealed record ConsumerLogEntry(LogLevel LogLevel, string Message, Exception Exception);
