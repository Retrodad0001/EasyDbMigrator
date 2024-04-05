// Ignore Spelling: Migrator

using EasyDbMigrator.DatabaseConnectors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//TODO .net 6 (LTS) support until 12 November 2024
//TODO .net 7 support until 14 May 2024 --> we will support this version until 12 November 2024
//TODO .net 8 (LTS) support until 10 November 2026
//TODO .net 9 ???

namespace EasyDbMigrator;

/// <summary>
/// The main class to run the migration scripts
/// </summary>
public class DbMigrator : IDbMigrator
{
    private readonly ILogger _logger;
    private readonly IDatabaseConnector _databaseConnector;
    private readonly IAssemblyResourceHelper _assemblyResourceHelper;
    private readonly IDirectoryHelper _directoryHelper;
    private readonly IDataTimeHelper _dataTimeHelper;
    private readonly List<string> _excludedScriptsList = new();

    /// <summary>
    /// Constructor for the DbMigrator
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="databaseConnector"></param>
    /// <param name="assemblyResourceHelper"></param>
    /// <param name="directoryHelper"></param>
    /// <param name="dataTimeHelper"></param>
    public DbMigrator(ILogger logger
        , IDatabaseConnector databaseConnector
        , IAssemblyResourceHelper assemblyResourceHelper
        , IDirectoryHelper directoryHelper
        , IDataTimeHelper dataTimeHelper)
    {

        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        ArgumentNullException.ThrowIfNull(databaseConnector);

        _databaseConnector = databaseConnector;

        ArgumentNullException.ThrowIfNull(assemblyResourceHelper);

        _assemblyResourceHelper = assemblyResourceHelper;

        ArgumentNullException.ThrowIfNull(directoryHelper);

        _directoryHelper = directoryHelper;

        ArgumentNullException.ThrowIfNull(dataTimeHelper);

        _dataTimeHelper = dataTimeHelper;
    }

    /// <summary>
    /// Create DBMigration object
    /// </summary>
    /// <param name="migrationConfiguration">the configuration settings DBMigrator uses to perform its tasks</param>
    /// <param name="logger">the ILogger logging object u want that the DBMigrator should use</param>
    /// <param name="databaseConnector">select SQL Server or PostgresSever Connection</param>
    /// <returns></returns>
    public static DbMigrator Create(MigrationConfiguration? migrationConfiguration
        , ILogger logger
        , IDatabaseConnector databaseConnector)
    {
        // ReSharper disable once HeapView.BoxingAllocation
        ArgumentNullException.ThrowIfNull(migrationConfiguration);

        ArgumentNullException.ThrowIfNull(logger);

        ArgumentNullException.ThrowIfNull(databaseConnector);

        DbMigrator result = new(logger
            , databaseConnector
            , new AssemblyResourceHelper()
            , new DirectoryHelper()
            , new DataTimeHelper());

        return result;
    }

    /// <summary>
    /// Create DBMigration
    /// </summary>
    /// <param name="migrationConfiguration">the configuration settings DBMigrator uses to perform its tasks</param>
    /// <param name="logger">the ILogger logging object u want that the DBMigrator should use</param>
    /// <param name="dataTimeHelperMock">used for mocking and testing time specific scenario's</param>
    /// <param name="databaseConnector">select SQL Server or PostgresSever Connection</param>
    /// <returns></returns>
    public static DbMigrator CreateForLocalIntegrationTesting(MigrationConfiguration? migrationConfiguration
        , ILogger logger
        , IDataTimeHelper dataTimeHelperMock
        , IDatabaseConnector databaseConnector)
    {
        // ReSharper disable once HeapView.BoxingAllocation
        ArgumentNullException.ThrowIfNull(migrationConfiguration);

        ArgumentNullException.ThrowIfNull(logger);

        ArgumentNullException.ThrowIfNull(dataTimeHelperMock);

        ArgumentNullException.ThrowIfNull(databaseConnector);

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
            _logger.Log(LogLevel.Warning,
                new StringBuilder().Append("migration process was canceled from the outside").ToString());
            return true;
        }

        LogBasicInformation(ref migrationConfiguration);

        var setupDatabaseAction = await TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(migrationConfiguration
            , cancellationToken).ConfigureAwait(false);

        if (setupDatabaseAction.HasFailure)
        {
            _logger.Log(LogLevel.Error, setupDatabaseAction.Exception, "setup database executed with errors");
            _logger.Log(LogLevel.Error, null, "migration process executed with errors");
            return false;
        }

        _logger.Log(LogLevel.Information, "setup database executed successfully");
        var createVersioningTableAction = await TrySetupDbMigrationsTableWhenNotExistAsync(migrationConfiguration
           , cancellationToken).ConfigureAwait(false);

        if (createVersioningTableAction.HasFailure)
        {
            _logger.Log(LogLevel.Error, createVersioningTableAction.Exception, "setup versioning table executed with errors");
            _logger.Log(LogLevel.Error, null, "migration process executed with errors");
            return false;
        }

        _logger.Log(LogLevel.Information, "setup versioning table executed successfully");

        var loadScriptsAction = await TryLoadingScripts(typeOfClassWhereScriptsAreLocated, migrationConfiguration).ConfigureAwait(false);

        if (loadScriptsAction.HasFailure)
        {
            _logger.Log(LogLevel.Error, loadScriptsAction.Exception, "One or more scripts could not be loaded, is the sequence patterns correct?");
            _logger.Log(LogLevel.Error, null, "migration process executed with errors");
            return false;
        }

#pragma warning disable CS8604 // Possible null reference argument.
        var runMigrationsScriptsAction = await TryRunMigrationScriptsAsync(migrationConfiguration
            , loadScriptsAction.Value
            , cancellationToken).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.

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

    private void LogBasicInformation(ref MigrationConfiguration migrationConfiguration)
    {
        // ReSharper disable once HeapView.ObjectAllocation
        _logger.Log(LogLevel.Information, "start running migrations for database: {migrationConfiguration.DatabaseName}", migrationConfiguration.DatabaseName);
        // ReSharper disable once HeapView.ObjectAllocation
        _logger.Log(LogLevel.Information, "connection-string used: {migrationConfiguration.ConnectionString}", migrationConfiguration.ConnectionString);
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

            // ReSharper disable once HeapView.ObjectAllocation
            // ReSharper disable once HeapView.BoxingAllocation
            _logger.Log(LogLevel.Information, "Total scripts found: {unOrderedScripts.Count}", unOrderedScripts.Count);
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
        foreach (Script? script in orderedScripts)
        {
            if (skipBecauseOfErrorWithPreviousScript)
            {
                // ReSharper disable once HeapView.ObjectAllocation
                _logger.Log(LogLevel.Warning, "script: {script.FileName} was skipped due to exception in previous script", script.FileName);
                continue;
            }

            DateTimeOffset executedDateTime = _dataTimeHelper.GetCurrentUtcTime();

            var result = await _databaseConnector.RunDbMigrationScriptAsync(migrationConfiguration
                , script
                , executedDateTime
                , cancellationToken).ConfigureAwait(false);

            if (result.Value == RunMigrationResult.MigrationWasCancelled)
            {
                _logger.Log(LogLevel.Warning, new StringBuilder().Append("migration process was canceled").ToString());
                return new Result<bool>(false);
            }
            else if (result.Value == RunMigrationResult.MigrationScriptExecuted)
            {
                // ReSharper disable once HeapView.ObjectAllocation
                _logger.Log(LogLevel.Information, "script: {script.FileName} was run", script.FileName);
            }
            else if (result.Value == RunMigrationResult.ScriptSkippedBecauseAlreadyRun)
            {
                // ReSharper disable once HeapView.ObjectAllocation
                _logger.Log(LogLevel.Information, "script: {script.FileName} was not run because script was already executed", script.FileName);
            }
            else if (result.Value == RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted)
            {
                // ReSharper disable once HeapView.ObjectAllocation
                _logger.Log(LogLevel.Error, result.Exception, "script: {script.FileName} was not completed due to exception", script.FileName);
                skipBecauseOfErrorWithPreviousScript = true;
            }
        }

        return skipBecauseOfErrorWithPreviousScript ? new Result<bool>(false) : new Result<bool>(true);
    }

    // ReSharper disable once HeapView.ClosureAllocation
    private static List<Script> RemoveExcludedScripts(IEnumerable<Script> scripts, IReadOnlyCollection<string> excludedScripts)
    {
        // ReSharper disable once HeapView.DelegateAllocation
        // ReSharper disable once HeapView.ClosureAllocation
        var result = scripts.Where(p => excludedScripts.All(x => x != p.FileName)).ToList();
        return result;
    }

    private static List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
    {
        return scripts.OrderBy(s => s.DatePartOfName)
            .ThenBy(s => s.SequenceNumberPart).ToList();
    }
}