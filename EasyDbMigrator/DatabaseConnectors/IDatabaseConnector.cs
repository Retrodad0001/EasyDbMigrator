using System;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public interface IDatabaseConnector
    {
        Task<Result<bool>> TryDeleteDatabaseIfExistAsync(string databaseName, string connectionString);
        Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(MigrationConfiguration migrationConfiguration);
        Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration);
        Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(MigrationConfiguration migrationConfiguration, Script script, DateTimeOffset executedDateTime);
        Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent);
    }
}