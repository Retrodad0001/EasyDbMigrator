// Ignore Spelling: Utc

using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator;

/// <summary>
/// Helper for getting the current time
/// </summary>
[ExcludeFromCodeCoverage] //tested in integrationTest
public sealed class DateTimeHelper : IDateTimeHelper
{
    /// <summary>
    /// gets the current Utc time
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }
}