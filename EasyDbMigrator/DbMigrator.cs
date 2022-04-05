using EasyDbMigrator.DatabaseConnectors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DbMigrator : IDbMigrator
    {
        private readonly ILogger _logger;
        private readonly IDatabaseConnector _databaseConnector;
        private readonly IAssemblyResourceHelper _assemblyResourceHelper;
        private readonly IDirectoryHelper _directoryHelper;
        private readonly IDataTimeHelper _dataTimeHelper;
        private readonly List<string> _excludedScriptsList = new();

        public DbMigrator(ILogger logger
            , IDatabaseConnector databaseConnector
            , IAssemblyResourceHelper assemblyResourceHelper
            , IDirectoryHelper directoryHelper
            , IDataTimeHelper dataTimeHelper)
        {

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;

            if (databaseConnector is null)
            {
                throw new ArgumentNullException(nameof(databaseConnector));
            }

            _databaseConnector = databaseConnector;

            if (assemblyResourceHelper is null)
            {
                throw new ArgumentNullException(nameof(assemblyResourceHelper));
            }

            _assemblyResourceHelper = assemblyResourceHelper;

            if (directoryHelper is null)
            {
                throw new ArgumentNullException(nameof(directoryHelper));
            }

            _directoryHelper = directoryHelper;

            if (dataTimeHelper is null)
            {
                throw new ArgumentNullException(nameof(dataTimeHelper));
            }

            _dataTimeHelper = dataTimeHelper;
        }

        /// <summary>
        /// Create DBMigration object so it can be used in code
        /// </summary>
        /// <param name="migrationConfiguration">the configuration settings DBMigrator uses to perform its tasks</param>
        /// <param name="logger">the ILogger logging object u want that the DBMigrator should use</param>
        /// <param name="databaseConnector">select SQL Server or PostgresSever Connection</param>
        /// <returns></returns>
        public static DbMigrator Create(MigrationConfiguration migrationConfiguration
            , ILogger logger
            , IDatabaseConnector databaseConnector)
        {
            if (migrationConfiguration is null)
            {
                throw new ArgumentNullException(nameof(migrationConfiguration));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (databaseConnector is null)
            {
                throw new ArgumentNullException(nameof(databaseConnector));
            }

            DbMigrator result = new(logger
                , databaseConnector
                , new AssemblyResourceHelper()
                , new DirectoryHelper()
                , new DataTimeHelper());

            return result;
        }

        /// <summary>
        /// Create DBMigration object so it can be used for integration testing
        /// </summary>
        /// <param name="migrationConfiguration">the configuration settings DBMigrator uses to perform its tasks</param>
        /// <param name="logger">the ILogger logging object u want that the DBMigrator should use</param>
        /// <param name="dataTimeHelperMock">used for mocking and testing time specific scenario's</param>
        /// <param name="databaseConnector">select SQL Server or PostgresSever Connection</param>
        /// <returns></returns>
        public static DbMigrator CreateForLocalIntegrationTesting(MigrationConfiguration migrationConfiguration
            , ILogger logger
            , IDataTimeHelper dataTimeHelperMock
            , IDatabaseConnector databaseConnector)
        {
            if (migrationConfiguration is null)
            {
                throw new ArgumentNullException(nameof(migrationConfiguration));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (dataTimeHelperMock is null)
            {
                throw new ArgumentNullException(nameof(dataTimeHelperMock));
            }

            if (databaseConnector is null)
            {
                throw new ArgumentNullException(nameof(databaseConnector));
            }

            DbMigrator result = new(logger
               , databaseConnector
               , new AssemblyResourceHelper()
               , new DirectoryHelper()
               , dataTimeHelperMock);

            return result;
        }

        /// <summary>
        /// If the database exist( specified by the parameter databaseName) 
        /// it will rollback all the transactions and drop the database. 
        /// Use this only for testing in non-production environments !
        /// </summary>
        /// <param name="migrationConfiguration">the settings to use for this migration</param>
        /// <param name="cancellationToken">The cancellation instruction</param>
        /// <returns></returns>
        public virtual async Task<bool> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken = default)
        {

            var succeeded = await _databaseConnector.TryDeleteDatabaseIfExistAsync(migrationConfiguration
                , cancellationToken).ConfigureAwait(false);

            if (succeeded.HasFailure)
            {
                _logger.Log(LogLevel.Error, succeeded.Exception, "DeleteDatabaseIfExistAsync executed with error");
                return false;
            }

            _logger.Log(LogLevel.Information, "DeleteDatabaseIfExistAsync has executed");
            return true;
        }

        /// <summary>
        /// Run all the migration scripts(embedded resources) specified in the 
        /// assembly where the type (see param typeOfClassWhereScriptsAreLocated) exist
        /// </summary>
        /// <param name="typeOfClassWhereScriptsAreLocated">the location of the migrationFiles</param>
        /// <param name="migrationConfiguration">the settings to use for this migration</param>
        /// <param name="cancellationToken">The cancellation instruction</param>
        /// <returns></returns>
        public virtual async Task<bool> TryApplyMigrationsAsync(Type typeOfClassWhereScriptsAreLocated
            , MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken = default)
        {
            if (IsCancellationRequested(cancellationToken))
            {
                _logger.Log(LogLevel.Warning, $"migration process was canceled from the outside");
                return true;
            }

            LogBasicInformation(migrationConfiguration);

            var setupDatabaseAction = await TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(migrationConfiguration
                , cancellationToken).ConfigureAwait(false);

            if (setupDatabaseAction.HasFailure)
            {
                _logger.Log(LogLevel.Error, setupDatabaseAction.Exception, @"setup database executed with errors");
                _logger.Log(LogLevel.Error, null, "migration process executed with errors");
                return false;
            }

            _logger.Log(LogLevel.Information, @"setup database executed successfully");
            var createVersioningTableAction = await TrySetupDbMigrationsTableWhenNotExistAsync(migrationConfiguration
               , cancellationToken).ConfigureAwait(false);

            if (createVersioningTableAction.HasFailure)
            {
                _logger.Log(LogLevel.Error, createVersioningTableAction.Exception, @"setup versioning table executed with errors");
                _logger.Log(LogLevel.Error, null, "migration process executed with errors");
                return false;
            }

            _logger.Log(LogLevel.Information, @"setup versioning table executed successfully");

            var loadScriptsAction = await TryLoadingScripts(typeOfClassWhereScriptsAreLocated, migrationConfiguration).ConfigureAwait(false);

            if (loadScriptsAction.HasFailure)
            {
                _logger.Log(LogLevel.Error, loadScriptsAction.Exception, "One or more scripts could not be loaded, is the sequence patterns correct?");
                _logger.Log(LogLevel.Error, null, "migration process executed with errors");
                return false;
            }

            var runMigrationsScriptsAction = await TryRunMigrationScriptsAsync(migrationConfiguration
#pragma warning disable CS8604 // Possible null reference argument.
                , loadScriptsAction.Value
#pragma warning restore CS8604 // Possible null reference argument.
                , cancellationToken).ConfigureAwait(false);

            if (runMigrationsScriptsAction.HasFailure)
            {
                _logger.Log(LogLevel.Error, null, "migration process executed with errors");
                return false;
            }

            _logger.Log(LogLevel.Information, "migration process executed successfully");
            return true;
        }

        /// <summary>
        /// Specificity the scripts (by name) what u want to exclude from the migration run
        /// </summary>
        /// <param name="scriptsToExcludeByName"></param>
        public virtual void ExcludeTheseScriptsInRun(List<string> scriptsToExcludeByName)
        {
            _excludedScriptsList.AddRange(scriptsToExcludeByName);
        }

        private static bool IsCancellationRequested(CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested;
        }

        private void LogBasicInformation(MigrationConfiguration migrationConfiguration)
        {
            _logger.Log(LogLevel.Information, $"start running migrations for database: {migrationConfiguration.DatabaseName}");
            _logger.Log(LogLevel.Information, $"connection-string used: {migrationConfiguration.ConnectionString}");
        }

        private async Task<Result<bool>> TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {
            var result = await _databaseConnector.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(migrationConfiguration
                , cancellationToken).ConfigureAwait(false);

            return result;
        }

        private async Task<Result<bool>> TrySetupDbMigrationsTableWhenNotExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {
            var result = await _databaseConnector.TrySetupDbMigrationsRunTableWhenNotExistAsync(migrationConfiguration
                , cancellationToken).ConfigureAwait(false);

            return result;
        }

        private async Task<Result<List<Script>>> TryLoadingScripts(Type typeOfClassWhereScriptsAreLocated, MigrationConfiguration migrationConfiguration)
        {
            try
            {
                List<Script> unOrderedScripts;
              
                if (string.IsNullOrEmpty(migrationConfiguration.ScriptsDirectory))
                {
                    unOrderedScripts = await _assemblyResourceHelper.TryGetScriptsFromAssembly(typeOfClassWhereScriptsAreLocated).ConfigureAwait(false);
                }
                else
                {
                    unOrderedScripts = await _directoryHelper.TryGetScriptsFromDirectoryAsync(migrationConfiguration.ScriptsDirectory).ConfigureAwait(false);
                }

                _logger.Log(LogLevel.Information, $"Total scripts found: {unOrderedScripts.Count}");
                var unOrderedScriptsWithoutExcludedScripts = RemoveExcludedScripts(unOrderedScripts, _excludedScriptsList);
                var orderedScriptsWithoutExcludedScripts = SetScriptsInCorrectSequence(unOrderedScriptsWithoutExcludedScripts);
                return new Result<List<Script>>(true, orderedScriptsWithoutExcludedScripts);
            }
            catch (Exception ex)
            {
                return new Result<List<Script>>(false, ex);
            }
        }

        private async Task<Result<bool>> TryRunMigrationScriptsAsync(MigrationConfiguration migrationConfiguration
            , List<Script> orderedScripts
            , CancellationToken cancellationToken)
        {
            bool skipBecauseOfErrorWithPreviousScript = false;
            foreach (var script in orderedScripts)
            {
                if (skipBecauseOfErrorWithPreviousScript)
                {
                    _logger.Log(LogLevel.Warning, $"script: {script.FileName} was skipped due to exception in previous script");
                    continue;
                }

                var executedDateTime = _dataTimeHelper.GetCurrentUtcTime();

                var result = await _databaseConnector.RunDbMigrationScriptAsync(migrationConfiguration
                    , script
                    , executedDateTime
                    , cancellationToken).ConfigureAwait(false);

                if (result.Value == RunMigrationResult.MigrationWasCancelled)
                {
                    _logger.Log(LogLevel.Warning, $"migration process was canceled");
                    return new Result<bool>(false);
                }
                else if (result.Value == RunMigrationResult.MigrationScriptExecuted)
                {
                    _logger.Log(LogLevel.Information, $"script: {script.FileName} was run");
                }
                else if (result.Value == RunMigrationResult.ScriptSkippedBecauseAlreadyRun)
                {
                    _logger.Log(LogLevel.Information, $"script: {script.FileName} was not run because script was already executed");
                }
                else if (result.Value == RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted)
                {
                    _logger.Log(LogLevel.Error, result.Exception, $"script: {script.FileName} was not completed due to exception");
                    skipBecauseOfErrorWithPreviousScript = true;
                }
            }

            return skipBecauseOfErrorWithPreviousScript ? new Result<bool>(false) : new Result<bool>(true);
        }

        private static List<Script> RemoveExcludedScripts(List<Script> scripts, List<string> excludedScripts)
        {
            var result = scripts.Where(p => excludedScripts.All(x => x != p.FileName)).ToList();
            return result;
        }

        private static List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
        {
            return scripts.OrderBy(s => s.DatePartOfName)
                .ThenBy(s => s.SequenceNumberPart).ToList();
        }
    }
}