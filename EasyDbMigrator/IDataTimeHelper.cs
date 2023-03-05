using System;

namespace EasyDbMigrator;

public interface IDataTimeHelper
{
    DateTimeOffset GetCurrentUtcTime();
}
