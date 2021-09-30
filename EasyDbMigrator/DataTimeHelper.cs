using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator
{
    [ExcludeFromCodeCoverage] //tested in integrationtest
    public class DataTimeHelper : IDataTimeHelper
    {
        public DateTimeOffset GetCurrentUtcTime()
        {
            return DateTime.UtcNow;
        }
    }
}
