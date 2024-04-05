using System;

namespace EasyDbMigrator;

/// <summary>
/// Interface for getting the current time.
/// </summary>
public interface IDataTimeHelper
{
    /// <summary>
    /// Get the current time in UTC.
    /// </summary>
    /// <returns></returns>
    DateTimeOffset GetCurrentUtcTime();
}
