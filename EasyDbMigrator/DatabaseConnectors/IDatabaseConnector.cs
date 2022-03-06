using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors
{
    public interface IDatabaseConnector
    {
        Task<Result<bool>> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken);
        Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken);

        Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken);

        Task<Result<RunMigrationResult>> RunDbMigrationScriptAsync(MigrationConfiguration migrationConfiguration
            , Script script
            , DateTimeOffset executedDateTime
            , CancellationToken cancellationToken);
    }
}