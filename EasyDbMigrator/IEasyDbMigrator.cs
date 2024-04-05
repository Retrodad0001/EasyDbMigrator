// Ignore Spelling: Migrator

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator;

/// <summary>
/// Interface for the EasyDbMigrator
/// </summary>
public interface IDbMigrator
{
    /// <summary>
    /// Tries to apply the migrations to the database
    /// </summary>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
       , CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to apply the migrations to the database
    /// </summary>
    /// <param name="typeOfClassWhereScriptsAreLocated"></param>
    /// <param name="migrationConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> TryApplyMigrationsAsync(Type typeOfClassWhereScriptsAreLocated
        , MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to apply the migrations to the database
    /// </summary>
    /// <param name="scriptsToExcludeByName"></param>
    void ExcludeTheseScriptsInRun(List<string> scriptsToExcludeByName);
}