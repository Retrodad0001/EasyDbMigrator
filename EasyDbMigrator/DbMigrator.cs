// Ignore Spelling: Migrator

using EasyDbMigrator.DatabaseConnectors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator;

//TODO remove .net 6 actions
//TODO add .net 9 actions
//TOOD add. net 9 target
//TOOD update packages


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

        ArgumentNullException.ThrowIfNull(argument: logger);

        _logger = logger;

        ArgumentNullException.ThrowIfNull(argument: databaseConnector);

        _databaseConnector = databaseConnector;

        ArgumentNullException.ThrowIfNull(argument: assemblyResourceHelper);

        _assemblyResourceHelper = assemblyResourceHelper;

        ArgumentNullException.ThrowIfNull(argument: directoryHelper);

        _directoryHelper = directoryHelper;

        ArgumentNullException.ThrowIfNull(argument: dataTimeHelper);

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
        ArgumentNullException.ThrowIfNull(argument: migrationConfiguration);

        ArgumentNullException.ThrowIfNull(argument: logger);

        ArgumentNullException.ThrowIfNull(argument: databaseConnector);

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
        ArgumentNullException.ThrowIfNull(argument: migrationConfiguration);

        ArgumentNullException.ThrowIfNull(argument: logger);

        ArgumentNullException.ThrowIfNull(argument: dataTimeHelperMock);

        ArgumentNullException.ThrowIfNull(argument: databaseConnector);

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
            _logger.Log(logLevel: LogLevel.Error, exception: succeeded.Exception, message: "DeleteDatabaseIfExistAsync executed with error");
            return false;
        }

        _logger.Log(logLevel: LogLevel.Information, message: "DeleteDatabaseIfExistAsync has executed");
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
#pragma warning disable CA2254 // Template should be a static expression
            _logger.Log(logLevel: LogLevel.Warning,
                message: new StringBuilder().Append("migration process was canceled from the outside").ToString());
#pragma warning restore CA2254 // Template should be a static expression
            return true;
        }

        LogBasicInformation(ref migrationConfiguration);

        var setupDatabaseAction = await TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(migrationConfiguration
            , cancellationToken).ConfigureAwait(false);

        if (setupDatabaseAction.HasFailure)
        {
            _logger.Log(logLevel: LogLevel.Error, exception: setupDatabaseAction.Exception, message: "setup database executed with errors");
            _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
            return false;
        }

        _logger.Log(logLevel: LogLevel.Information, message: "setup database executed successfully");
        var createVersioningTableAction = await TrySetupDbMigrationsTableWhenNotExistAsync(migrationConfiguration
           , cancellationToken).ConfigureAwait(false);

        if (createVersioningTableAction.HasFailure)
        {
            _logger.Log(logLevel: LogLevel.Error, exception: createVersioningTableAction.Exception, message: "setup versioning table executed with errors");
            _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
            return false;
        }

        _logger.Log(logLevel: LogLevel.Information, message: "setup versioning table executed successfully");

        var loadScriptsAction = await TryLoadingScripts(typeOfClassWhereScriptsAreLocated, migrationConfiguration).ConfigureAwait(false);

        if (loadScriptsAction.HasFailure)
        {
            _logger.Log(logLevel: LogLevel.Error, exception: loadScriptsAction.Exception, message: "One or more scripts could not be loaded, is the sequence patterns correct?");
            _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
            return false;
        }

#pragma warning disable CS8604 // Possible null reference argument.
        var runMigrationsScriptsAction = await TryRunMigrationScriptsAsync(migrationConfiguration
            , loadScriptsAction.Value
            , cancellationToken).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.

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
        _logger.Log(logLevel: LogLevel.Information, message: "start running migrations for database: {migrationConfiguration.DatabaseName}", args: migrationConfiguration.DatabaseName);
        _logger.Log(logLevel: LogLevel.Information, message: "connection-string used: {migrationConfiguration.ConnectionString}", args: migrationConfiguration.ConnectionString);
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

            _logger.Log(logLevel: LogLevel.Information, message: "Total scripts found: {unOrderedScripts.Count}", args: unOrderedScripts.Count);
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
                _logger.Log(logLevel: LogLevel.Warning, message: "script: {script.FileName} was skipped due to exception in previous script", args: script.FileName);
                continue;
            }

            DateTimeOffset executedDateTime = _dataTimeHelper.GetCurrentUtcTime();

            var result = await _databaseConnector.RunDbMigrationScriptAsync(migrationConfiguration
                , script
                , executedDateTime
                , cancellationToken).ConfigureAwait(false);

            if (result.Value == RunMigrationResult.MigrationWasCancelled)
            {
#pragma warning disable CA2254 // Template should be a static expression
                _logger.Log(logLevel: LogLevel.Warning, message: new StringBuilder().Append("migration process was canceled").ToString());
#pragma warning restore CA2254 // Template should be a static expression
                return new Result<bool>(false);
            }
            else if (result.Value == RunMigrationResult.MigrationScriptExecuted)
            {
                _logger.Log(logLevel: LogLevel.Information, message: "script: {script.FileName} was run", args: script.FileName);
            }
            else if (result.Value == RunMigrationResult.ScriptSkippedBecauseAlreadyRun)
            {
                _logger.Log(logLevel: LogLevel.Information, message: "script: {script.FileName} was not run because script was already executed", args: script.FileName);
            }
            else if (result.Value == RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted)
            {
                _logger.Log(logLevel: LogLevel.Error, exception: result.Exception, message: "script: {script.FileName} was not completed due to exception", args: script.FileName);
                skipBecauseOfErrorWithPreviousScript = true;
            }
        }

        return skipBecauseOfErrorWithPreviousScript ? new Result<bool>(false) : new Result<bool>(true);
    }

    private static List<Script> RemoveExcludedScripts(IEnumerable<Script> scripts, IReadOnlyCollection<string> excludedScripts)
    {
        var result = scripts.Where(p => excludedScripts.All(x => x != p.FileName)).ToList();
        return result;
    }

    private static List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
    {
        return scripts.OrderBy(s => s.DatePartOfName)
                      .ThenBy(s => s.SequenceNumberPart)
                      .ToList();
    }
}