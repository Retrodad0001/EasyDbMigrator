// Ignore Spelling: Utc

using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator;

[ExcludeFromCodeCoverage] //tested in integrationTest
public sealed class DataTimeHelper : IDataTimeHelper
{
    public DateTimeOffset GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }
}