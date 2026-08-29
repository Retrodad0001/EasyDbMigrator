// Ignore Spelling: Diagnostics Migrator
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyDbMigratorTests.TestHelpers;

/// <summary>
/// Collects everything needed to explain why an integration test failed:
/// the steps the test walked through (with timings) and every message the
/// migrator logged. The integration tests hand the migrator a mocked ILogger,
/// so without this the error messages and exceptions the migrator writes are
/// swallowed by the mock and a failure only shows "expected true, actual false".
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TestDiagnostics
{
    private readonly TestContext _testContext;
    private readonly string _testName;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly List<string> _steps = new();
    private readonly List<CapturedLog> _capturedLogs = new();
    private int _stepNumber;

    public TestDiagnostics(TestContext testContext, [CallerMemberName] string testName = "")
    {
        _testContext = testContext;
        _testName = testName;
        Step("test started");
    }

    /// <summary>
    /// Record (and immediately print) that the test reached a certain point.
    /// When something later blows up, the last recorded step tells you where.
    /// </summary>
    public void Step(string message)
    {
        _stepNumber++;
        string line = new StringBuilder()
            .Append("[STEP ").Append(_stepNumber.ToString("00", CultureInfo.InvariantCulture))
            .Append(" @ ").Append(_stopwatch.Elapsed.TotalSeconds.ToString("00.000", CultureInfo.InvariantCulture))
            .Append("s] ").Append(message)
            .ToString();

        _steps.Add(line);
        WriteLine(line);
    }

    /// <summary>
    /// Same as <see cref="Step(string)"/> for a step that carries a value,
    /// for example the generated database name or a connection string.
    /// </summary>
    public void Step(string message, object detail)
    {
        Step(new StringBuilder().Append(message).Append(": ").Append(detail).ToString());
    }

    /// <summary>
    /// Creates the ILogger mock the migrator should use. Everything the migrator
    /// logs through it is written to the test output right away and kept for the
    /// failure report, while the mock stays fully verifiable with
    /// CheckIfLoggerWasCalled.
    /// </summary>
    /// <param name="label">tells the loggers apart when a test runs the migrator more than once</param>
    public Mock<ILogger<T>> CreateLoggerMock<T>(string label)
    {
        Mock<ILogger<T>> loggerMock = new();
        CapturedLog captured = new(label);
        _capturedLogs.Add(captured);

#pragma warning disable CA1873 // Avoid potentially expensive logging
        _ = loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>()
                , It.IsAny<EventId>()
                , It.IsAny<It.IsAnyType>()
                , It.IsAny<Exception>()
                , It.IsAny<Func<It.IsAnyType, Exception, string>>()))
#pragma warning restore CA1873 // Avoid potentially expensive logging
            .Callback(new InvocationAction(invocation =>
            {
                LogLevel logLevel = (LogLevel)invocation.Arguments[0];
                object state = invocation.Arguments[2];
                Exception? exception = invocation.Arguments[3] as Exception;

                StringBuilder builder = new StringBuilder()
                    .Append("[LOG ").Append(label).Append(" @ ")
                    .Append(_stopwatch.Elapsed.TotalSeconds.ToString("00.000", CultureInfo.InvariantCulture))
                    .Append("s] ").Append(logLevel).Append(": ")
                    .Append(state?.ToString() ?? "<null state>");

                if (exception is not null)
                {
                    _ = builder.Append(Environment.NewLine)
                        .Append("        exception: ").Append(exception);
                }

                string line = builder.ToString();
                captured.Lines.Add(line);
                WriteLine(line);
            }));

        return loggerMock;
    }

    /// <summary>
    /// Assert.IsTrue that says which step failed and shows everything the
    /// migrator logged up to that point.
    /// </summary>
    public void AssertIsTrue(bool condition, string what)
    {
        if (condition)
        {
            Step(new StringBuilder().Append("OK: ").Append(what).ToString());
            return;
        }

        Assert.Fail(BuildReport(new StringBuilder()
            .Append("expected TRUE but got FALSE: ").Append(what).ToString()));
    }

    /// <summary>
    /// Assert.IsFalse that says which step failed and shows everything the
    /// migrator logged up to that point.
    /// </summary>
    public void AssertIsFalse(bool condition, string what)
    {
        if (!condition)
        {
            Step(new StringBuilder().Append("OK: ").Append(what).ToString());
            return;
        }

        Assert.Fail(BuildReport(new StringBuilder()
            .Append("expected FALSE but got TRUE: ").Append(what).ToString()));
    }

    /// <summary>
    /// Runs the given verification and, when it fails, appends the step history
    /// and the captured log lines to the raw Moq/MSTest failure message.
    /// </summary>
    public void Verify(string what, Action verification)
    {
        ArgumentNullException.ThrowIfNull(verification);

        try
        {
            verification();
        }
        catch (Exception ex)
        {
            Assert.Fail(BuildReport(new StringBuilder()
                .Append("verification failed: ").Append(what)
                .Append(Environment.NewLine)
                .Append(ex.Message)
                .ToString()));
        }

        Step(new StringBuilder().Append("OK: ").Append(what).ToString());
    }

    /// <summary>
    /// Use in the catch-all of a test: turns a bare exception into a report that
    /// also shows how far the test got and what the migrator logged.
    /// </summary>
    public string Describe(Exception exception)
    {
        return BuildReport(new StringBuilder()
            .Append("unexpected ").Append(exception?.GetType().Name ?? "exception")
            .Append(Environment.NewLine)
            .Append(exception)
            .ToString());
    }

    private string BuildReport(string reason)
    {
        StringBuilder report = new StringBuilder()
            .Append(Environment.NewLine)
            .Append("=== ").Append(_testName).Append(" FAILED ===").Append(Environment.NewLine)
            .Append(reason).Append(Environment.NewLine)
            .Append(Environment.NewLine)
            .Append("--- steps taken ---").Append(Environment.NewLine);

        foreach (string step in _steps)
        {
            _ = report.Append(step).Append(Environment.NewLine);
        }

        foreach (CapturedLog captured in _capturedLogs)
        {
            _ = report.Append(Environment.NewLine)
                .Append("--- migrator log (").Append(captured.Label).Append(") ---")
                .Append(Environment.NewLine);

            if (captured.Lines.Count == 0)
            {
                _ = report.Append("<nothing was logged>").Append(Environment.NewLine);
                continue;
            }

            foreach (string line in captured.Lines)
            {
                _ = report.Append(line).Append(Environment.NewLine);
            }
        }

        return report.ToString();
    }

    private void WriteLine(string line)
    {
        _testContext?.WriteLine(line);
        Console.WriteLine(line);
    }

    [ExcludeFromCodeCoverage]
    private sealed class CapturedLog
    {
        public CapturedLog(string label)
        {
            Label = label;
        }

        public string Label { get; }

        public List<string> Lines { get; } = new();
    }
}
