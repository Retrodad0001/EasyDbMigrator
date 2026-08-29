#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EasyDbMigratorTests.TestHelpers;

/// <summary>
/// Asks the docker CLI what it is doing. When a container never becomes ready
/// the exception from the test framework only says it gave up waiting, while the
/// container log usually says exactly why it refused to start.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DockerDiagnostics
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(20);

    public static string DescribeContainers()
    {
        string output = RunDocker("ps -a --format {{.Names}}|{{.Image}}|{{.Status}}");
        return string.IsNullOrWhiteSpace(output) ? "<no containers>" : output;
    }

    /// <summary>
    /// The test framework prefixes the container name with the environment name,
    /// so the container is looked up by prefix rather than by exact name.
    /// </summary>
    public static string DescribeContainerLogs(string containerNamePrefix, int tailLines)
    {
        StringBuilder builder = new();

        string names = RunDocker(new StringBuilder()
            .Append("ps -a --filter name=").Append(containerNamePrefix).Append(" --format {{.Names}}")
            .ToString());

        if (string.IsNullOrWhiteSpace(names))
        {
            return new StringBuilder()
                .Append("no container found whose name starts with ").Append(containerNamePrefix)
                .ToString();
        }

        foreach (string name in names.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = name.Trim();
            _ = builder.Append("--- docker logs ").Append(trimmed).Append(" (last ")
                .Append(tailLines).Append(" lines) ---").Append(Environment.NewLine)
                .Append(RunDocker(new StringBuilder()
                    .Append("logs --tail ").Append(tailLines).Append(' ').Append(trimmed)
                    .ToString()))
                .Append(Environment.NewLine);
        }

        return builder.ToString();
    }

    public static string DescribeHost()
    {
        long availableBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

        return new StringBuilder()
            .Append("os: ").Append(Environment.OSVersion)
            .Append(" | processors: ").Append(Environment.ProcessorCount)
            .Append(" | memory available: ").Append(availableBytes / 1024 / 1024).Append(" MB")
            .Append(" | docker server: ").Append(RunDocker("version --format {{.Server.Version}}"))
            .ToString();
    }

    /// <summary>
    /// How many containers are already running. The integration tests run in
    /// parallel, so this says how much company a container has while it boots,
    /// which is the first thing to look at when one times out becoming ready.
    /// </summary>
    public static int CountRunningContainers()
    {
        string output = RunDocker("ps --quiet");

        return string.IsNullOrWhiteSpace(output)
            ? 0
            : output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static string RunDocker(string arguments)
    {
        try
        {
            ProcessStartInfo startInfo = new("docker", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(startInfo);
            if (process is null)
            {
                return "<could not start the docker cli>";
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!process.WaitForExit((int)CommandTimeout.TotalMilliseconds))
            {
                return "<the docker cli did not respond in time>";
            }

            List<string> parts = new();
            if (!string.IsNullOrWhiteSpace(output))
            {
                parts.Add(output.TrimEnd());
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                parts.Add(new StringBuilder().Append("stderr: ").Append(error.TrimEnd()).ToString());
            }

            return string.Join(Environment.NewLine, parts);
        }
        catch (Exception ex)
        {
            return new StringBuilder().Append("<could not ask the docker cli: ").Append(ex.Message).Append('>').ToString();
        }
    }
}
