using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator
{
    [ExcludeFromCodeCoverage] //tested in integrationtest
    public sealed class DataTimeHelper : IDataTimeHelper
    {
        public DateTimeOffset GetCurrentUtcTime()
        {
            return DateTime.UtcNow;
        }
    }
}