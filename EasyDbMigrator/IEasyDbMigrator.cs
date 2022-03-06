using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public interface IDbMigrator
    {
        Task<bool> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
           , CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> TryApplyMigrationsAsync(Type typeOfClassWhereScriptsAreLocated
            , MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken = default(CancellationToken));

        void ExcludeTheseScriptsInRun(List<string> scriptsToExcludeByName);
    }
}