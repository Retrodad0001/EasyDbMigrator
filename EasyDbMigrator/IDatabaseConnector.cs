﻿using System;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public interface IDatabaseConnector
    {
        Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(MigrationConfiguration migrationConfiguration, SqlScript script, DateTimeOffset executedDateTime);
        Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent);
    }
}