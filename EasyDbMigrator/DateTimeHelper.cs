using System;

namespace EasyDbMigrator;

/// <summary>
/// Helper for getting the current time
/// </summary>
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