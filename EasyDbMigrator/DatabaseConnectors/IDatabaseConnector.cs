using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public interface IDatabaseConnector
    {
        Task<Result<bool>> TryDeleteDatabaseIfExistAsync(string databaseName, string connectionString, CancellationToken cancellationToken);
        Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(MigrationConfiguration migrationConfiguration, CancellationToken cancellationToken);
        Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration, CancellationToken cancellationToken);
        Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(MigrationConfiguration migrationConfiguration, Script script, DateTimeOffset executedDateTime, CancellationToken cancellationToken);
        Task<Result<bool>> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent, CancellationToken cancellationToken);
    }
}