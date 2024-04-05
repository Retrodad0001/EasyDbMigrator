using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator.DatabaseConnectors;

/// <summary>
/// Interface for database connectors
/// </summary>
public interface IDatabaseConnector
{
    /// <summary>
    /// Tries to delete the database if it exists
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<bool>> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken);
    /// <summary>
    /// Tries to setup the DbMigrationsRun table if it does not exist
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken);

    /// <summary>
    /// Tries to setup an empty database with default settings when there is no database
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<bool>> TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken);

    /// <summary>
    /// Tries to run a single script
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="script"></param>
    /// <param name="executedDateTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<RunMigrationResult>> RunDbMigrationScriptAsync(MigrationConfiguration migrationConfiguration
        , Script script
        , DateTimeOffset executedDateTime
        , CancellationToken cancellationToken);
}