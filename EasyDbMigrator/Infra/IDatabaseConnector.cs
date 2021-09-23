using EasyDbMigrator.Helpers;
using System;
using System.Threading.Tasks;

namespace EasyDbMigrator.Infra
{
    public interface IDatabaseConnector
    {
        Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(MigrationConfiguration migrationConfiguration, Script script, DateTime executedDateTime);
        Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent);
    }
}