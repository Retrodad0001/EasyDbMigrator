using EasyDbMigrator.DatabaseConnectors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public class DbMigrator : IDbMigrator
    {
        private readonly ILogger _logger;
        private readonly IDatabaseConnector _databaseconnector;
        private readonly IAssemblyResourceHelper _assemblyResourceHelper;
        private readonly IDirectoryHelper _directoryHelper;
        private readonly IDataTimeHelper _dataTimeHelper;
        private readonly List<string> _excludedScriptList = new List<string>();

        public DbMigrator(ILogger logger
            , IDatabaseConnector databaseconnector
            , IAssemblyResourceHelper assemblyResourceHelper
            , IDirectoryHelper directoryHelper
            , IDataTimeHelper dataTimeHelper)
        {

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));
            _logger = logger;

            if (databaseconnector is null)
                throw new ArgumentNullException(nameof(databaseconnector));
            _databaseconnector = databaseconnector;

            if (assemblyResourceHelper is null)
                throw new ArgumentNullException(nameof(assemblyResourceHelper));
            _assemblyResourceHelper = assemblyResourceHelper;

            if (directoryHelper is null)
                throw new ArgumentNullException(nameof(directoryHelper));
           
            _directoryHelper = directoryHelper;
            
            if (dataTimeHelper is null)
                throw new ArgumentNullException(nameof(dataTimeHelper));
            _dataTimeHelper = dataTimeHelper;
        }

        /// <summary>
        /// Create DBMigration object so it can be used in code
        /// </summary>
        /// <param name="migrationConfiguration">the configuration settings DBMigrator uses to perform its tasks</param>
        /// <param name="logger">the ILogger logging object u want that the DBMigrator should use</param>
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

            DbMigrator result = new DbMigrator(logger: logger
                , databaseconnector: databaseConnector
                , assemblyResourceHelper: new AssemblyResourceHelper()
                , directoryHelper: new DirectoryHelper()
                , dataTimeHelper: new DataTimeHelper());

            return result;
        }

        /// <summary>
        /// Create DBMigration object so it can be used for integration testing
        /// </summary>
        /// <param name="migrationConfiguration">the configuration settings DBMigrator uses to perform its tasks</param>
        /// <param name="logger">the ILogger logging object u want that the DBMigrator should use</param>
        /// <param name="dataTimeHelperMock">used for mocking and testing time specific scenario's </param>
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

            DbMigrator result = new DbMigrator(logger: logger
               , databaseconnector: databaseConnector
               , assemblyResourceHelper: new AssemblyResourceHelper()
               , directoryHelper: new DirectoryHelper()
               , dataTimeHelper: dataTimeHelperMock);

            return result;
        }

        /// <summary>
        /// If the database exist( specified by the parameter databasename) 
        /// it will rollback all the transactions and drop the database. 
        /// Use this only for testing in non-production environments !
        /// </summary>
        /// <param name="databaseName">the name of the database u want to drop</param>
        /// <param name="connectionString">the connection-string to the database-server</param>
        /// <param name="cancellationToken">The cancellation instruction</param>
        /// <returns></returns>
        public virtual async Task<bool> TryDeleteDatabaseIfExistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken = default)
        {

            Result<bool> succeeded = await _databaseconnector.TryDeleteDatabaseIfExistAsync(migrationConfiguration: migrationConfiguration
                , cancellationToken: cancellationToken).ConfigureAwait(true);

            if (succeeded.HasFailure)
            {
                _logger.Log(logLevel: LogLevel.Error, exception: succeeded.Exception, message: "DeleteDatabaseIfExistAsync executed with error");
                return false;
            }

            _logger.Log(logLevel: LogLevel.Information, message: "DeleteDatabaseIfExistAsync has executed");
            return true;
        }

        //TODO add test no scripts found with directory

        /// <summary>
        /// Run all the migration scripts(embedded resources) specified in the 
        /// assembly where the type (see param typeOfClassWhereScriptsAreLocated) exist
        /// </summary>
        /// <param name="typeWhereMigrationsScriptsExists"></param>
        /// <param name="cancellationToken">The cancellation instruction</param>
        /// <returns></returns>
        public virtual async Task<bool> TryApplyMigrationsAsync(Type typeOfClassWhereScriptsAreLocated
            , MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken = default)
        {
            if (IsCancellationRequested(cancellationToken))
            {
                _logger.Log(logLevel: LogLevel.Warning, message: $"migration process was canceled from the outside");
                return true;
            }

            LogBasicInformation(migrationConfiguration);

            Result<bool> setupDatabaseAction = await TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(migrationConfiguration: migrationConfiguration
                , cancellationToken: cancellationToken).ConfigureAwait(true);

            if (setupDatabaseAction.HasFailure)
            {
                _logger.Log(logLevel: LogLevel.Error, exception: setupDatabaseAction.Exception, @"setup database executed with errors");
                _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
                return false;
            }

            _logger.Log(logLevel: LogLevel.Information, message: @"setup database executed successfully");
            Result<bool> createVersioningTableAction = await TrySetupDbMigrationsRunTableWhenNotExcistAsync(migrationConfiguration: migrationConfiguration
               , cancellationToken: cancellationToken).ConfigureAwait(true);

            if (createVersioningTableAction.HasFailure)
            {
                _logger.Log(logLevel: LogLevel.Error, exception: createVersioningTableAction.Exception, @"setup versioning table executed with errors");
                _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
                return false;
            }

            _logger.Log(logLevel: LogLevel.Information, message: @"setup versioning table executed successfully");

            Result<List<Script>> loadScriptsAction = await TryLoadingScripts(typeOfClassWhereScriptsAreLocated, migrationConfiguration).ConfigureAwait(true);

            if (loadScriptsAction.HasFailure)
            {
                _logger.Log(logLevel: LogLevel.Error, exception: loadScriptsAction.Exception, message: "One or more scripts could not be loaded, is the sequence patterns correct?");
                _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
                return false;
            }

            Result<bool> runMigrationsScriptsAction = await TryRunAllMigrationScriptsAsync(migrationConfiguration: migrationConfiguration
                , orderedScripts: loadScriptsAction.Value
                , cancellationToken: cancellationToken).ConfigureAwait(true);

            if (runMigrationsScriptsAction.HasFailure)
            {
                _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
                return false;
            }

            _logger.Log(logLevel: LogLevel.Information, message: "migration process executed successfully");
            return true;
        }

        /// <summary>
        /// Specificity the scripts (by name) what u want to exclude from the migration run
        /// </summary>
        /// <param name="scriptsToExclude"></param>
        public virtual void ExcludeTheseScriptsInRun(List<string> scriptsToExcludeByname)
        {
            _excludedScriptList.AddRange(scriptsToExcludeByname);
        }

        private static bool IsCancellationRequested(CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested;
        }

        private void LogBasicInformation(MigrationConfiguration migrationConfiguration)
        {
            _logger.Log(logLevel: LogLevel.Information, message: $"start running migrations for database: {migrationConfiguration.DatabaseName}");
            _logger.Log(logLevel: LogLevel.Information, message: $"connection-string used: {migrationConfiguration.ConnectionString}");
        }

        private async Task<Result<bool>> TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {
            var result = await _databaseconnector.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(migrationConfiguration: migrationConfiguration
                , cancellationToken: cancellationToken).ConfigureAwait(true);

            return result;
        }

        private async Task<Result<bool>> TrySetupDbMigrationsRunTableWhenNotExcistAsync(MigrationConfiguration migrationConfiguration
            , CancellationToken cancellationToken)
        {
            var result = await _databaseconnector.TrySetupDbMigrationsRunTableWhenNotExcistAsync(migrationConfiguration: migrationConfiguration
                , cancellationToken: cancellationToken).ConfigureAwait(true);

            return result;
        }

        private async Task<Result<List<Script>>> TryLoadingScripts(Type typeOfClassWhereScriptsAreLocated, MigrationConfiguration migrationConfiguration)
        {
            List<Script> unOrderedScripts;

            try
            {
                if (string.IsNullOrEmpty(migrationConfiguration.ScriptsDirectory))
                    unOrderedScripts = await _assemblyResourceHelper.TryConverManifestResourceStreamsToScriptsAsync(typeOfClassWhereScriptsAreLocated: typeOfClassWhereScriptsAreLocated).ConfigureAwait(true);
                else
                    unOrderedScripts = await _directoryHelper.TryGetScriptsFromDirectoryAsync(migrationConfiguration.ScriptsDirectory).ConfigureAwait(true);

                _logger.Log(logLevel: LogLevel.Information, message: $"Total scripts found: {unOrderedScripts.Count}");
                List<Script> unOrderedScriptsWithoutExludedScripts = RemoveExcludedScripts(scripts: unOrderedScripts, excludedscripts: _excludedScriptList);
                List<Script> orderedScriptsWithoutExcludedScripts = SetScriptsInCorrectSequence(scripts: unOrderedScriptsWithoutExludedScripts);
                return new Result<List<Script>>(wasSuccessful: true, value: orderedScriptsWithoutExcludedScripts);
            }
            catch (Exception ex)
            {
                return new Result<List<Script>>(wasSuccessful: false, exception: ex);
            }
        }

        private async Task<Result<bool>> TryRunAllMigrationScriptsAsync(MigrationConfiguration migrationConfiguration
            , List<Script> orderedScripts
            , CancellationToken cancellationToken)
        {
            bool skipBecauseOfErrorWithPreviousScript = false;
            foreach (Script script in orderedScripts)
            {
                if (skipBecauseOfErrorWithPreviousScript)
                {
                    _logger.Log(logLevel: LogLevel.Warning, message: $"script: {script.FileName} was skipped due to exception in previous script");
                    continue;
                }

                DateTimeOffset executedDateTime = _dataTimeHelper.GetCurrentUtcTime();

                Result<RunMigrationResult> result = await _databaseconnector.RunDbMigrationScriptWhenNotRunnedBeforeAsync(migrationConfiguration: migrationConfiguration
                    , script: script
                    , executedDateTime: executedDateTime
                    , cancellationToken: cancellationToken).ConfigureAwait(true);

                if (result.Value == RunMigrationResult.MigrationWasCancelled)
                {
                    _logger.Log(logLevel: LogLevel.Warning, message: $"Whole migration process was canceled");
                    return new Result<bool>(wasSuccessful: true);
                }
                else if (result.Value == RunMigrationResult.MigrationScriptExecuted)
                {
                    _logger.Log(logLevel: LogLevel.Information, message: $"script: {script.FileName} was run");
                }
                else if (result.Value == RunMigrationResult.ScriptSkippedBecauseAlreadyRun)
                {
                    _logger.Log(logLevel: LogLevel.Information, message: $"script: {script.FileName} was not run because script was already executed");
                }
                else if (result.Value == RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted)
                {
                    _logger.Log(logLevel: LogLevel.Error, exception: result.Exception, message: $"script: {script.FileName} was not completed due to exception");
                    skipBecauseOfErrorWithPreviousScript = true;
                }
            }

            if (skipBecauseOfErrorWithPreviousScript)
                return new Result<bool>(wasSuccessful: false);
            else
                return new Result<bool>(wasSuccessful: true); ;
        }

        private static List<Script> RemoveExcludedScripts(List<Script> scripts, List<string> excludedscripts)
        {
            var result = scripts.Where(p => !excludedscripts.Any(x => x == p.FileName)).ToList();
            return result;
        }

        private static List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
        {
            return scripts.OrderBy(s => s.DatePartOfName)
                .ThenBy(s => s.SequenceNumberPart).ToList();
        }
    }
}

//TODO !!! can customize the script sequence pattern
//TODO !!! has support for .net 6