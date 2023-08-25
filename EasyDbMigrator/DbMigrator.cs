// Ignore Spelling: Migrator

using EasyDbMigrator.DatabaseConnectors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDbMigrator;

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
            throw new ArgumentNullException(paramName: nameof(logger));
        }

        _logger = logger;

        if (databaseConnector is null)
        {
            throw new ArgumentNullException(paramName: nameof(databaseConnector));
        }

        _databaseConnector = databaseConnector;

        if (assemblyResourceHelper is null)
        {
            throw new ArgumentNullException(paramName: nameof(assemblyResourceHelper));
        }

        _assemblyResourceHelper = assemblyResourceHelper;

        if (directoryHelper is null)
        {
            throw new ArgumentNullException(paramName: nameof(directoryHelper));
        }

        _directoryHelper = directoryHelper;

        if (dataTimeHelper is null)
        {
            throw new ArgumentNullException(paramName: nameof(dataTimeHelper));
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
            throw new ArgumentNullException(paramName: nameof(migrationConfiguration));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(paramName: nameof(logger));
        }

        if (databaseConnector is null)
        {
            throw new ArgumentNullException(paramName: nameof(databaseConnector));
        }

        DbMigrator result = new(logger: logger
            , databaseConnector: databaseConnector
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
            throw new ArgumentNullException(paramName: nameof(migrationConfiguration));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(paramName: nameof(logger));
        }

        if (dataTimeHelperMock is null)
        {
            throw new ArgumentNullException(paramName: nameof(dataTimeHelperMock));
        }

        if (databaseConnector is null)
        {
            throw new ArgumentNullException(paramName: nameof(databaseConnector));
        }

        DbMigrator result = new(logger: logger
           , databaseConnector: databaseConnector
           , assemblyResourceHelper: new AssemblyResourceHelper()
           , directoryHelper: new DirectoryHelper()
           , dataTimeHelper: dataTimeHelperMock);

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

        var succeeded = await _databaseConnector.TryDeleteDatabaseIfExistAsync(migrationConfiguration: migrationConfiguration
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

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
        if (IsCancellationRequested(cancellationToken: cancellationToken))
        {
            _logger.Log(logLevel: LogLevel.Warning, message: $"migration process was canceled from the outside");
            return true;
        }

        LogBasicInformation(migrationConfiguration: migrationConfiguration);

        var setupDatabaseAction = await TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(migrationConfiguration: migrationConfiguration
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        if (setupDatabaseAction.HasFailure)
        {
            _logger.Log(logLevel: LogLevel.Error, exception: setupDatabaseAction.Exception, message: @"setup database executed with errors");
            _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
            return false;
        }

        _logger.Log(logLevel: LogLevel.Information, message: @"setup database executed successfully");
        var createVersioningTableAction = await TrySetupDbMigrationsTableWhenNotExistAsync(migrationConfiguration: migrationConfiguration
           , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        if (createVersioningTableAction.HasFailure)
        {
            _logger.Log(logLevel: LogLevel.Error, exception: createVersioningTableAction.Exception, message: @"setup versioning table executed with errors");
            _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
            return false;
        }

        _logger.Log(logLevel: LogLevel.Information, message: @"setup versioning table executed successfully");

        var loadScriptsAction = await TryLoadingScripts(typeOfClassWhereScriptsAreLocated: typeOfClassWhereScriptsAreLocated, migrationConfiguration: migrationConfiguration).ConfigureAwait(continueOnCapturedContext: false);

        if (loadScriptsAction.HasFailure)
        {
            _logger.Log(logLevel: LogLevel.Error, exception: loadScriptsAction.Exception, message: "One or more scripts could not be loaded, is the sequence patterns correct?");
            _logger.Log(logLevel: LogLevel.Error, exception: null, message: "migration process executed with errors");
            return false;
        }

#pragma warning disable CS8604 // Possible null reference argument.
        var runMigrationsScriptsAction = await TryRunMigrationScriptsAsync(migrationConfiguration: migrationConfiguration
            , orderedScripts: loadScriptsAction.Value
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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
        _excludedScriptsList.AddRange(collection: scriptsToExcludeByName);
    }

    private static bool IsCancellationRequested(CancellationToken cancellationToken)
    {
        return cancellationToken.IsCancellationRequested;
    }

    private void LogBasicInformation(MigrationConfiguration migrationConfiguration)
    {
        _logger.Log(logLevel: LogLevel.Information, message: "start running migrations for database: {migrationConfiguration.DatabaseName}", args: migrationConfiguration.DatabaseName);
        _logger.Log(logLevel: LogLevel.Information, message: "connection-string used: {migrationConfiguration.ConnectionString}", args: migrationConfiguration.ConnectionString);
    }

    private async Task<Result<bool>> TrySetupEmptyDataBaseWhenThereIsNoDatabaseAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {
        var result = await _databaseConnector.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(migrationConfiguration: migrationConfiguration
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        return result;
    }

    private async Task<Result<bool>> TrySetupDbMigrationsTableWhenNotExistAsync(MigrationConfiguration migrationConfiguration
        , CancellationToken cancellationToken)
    {
        var result = await _databaseConnector.TrySetupDbMigrationsRunTableWhenNotExistAsync(migrationConfiguration: migrationConfiguration
            , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        return result;
    }

    private async Task<Result<List<Script>>> TryLoadingScripts(Type typeOfClassWhereScriptsAreLocated, MigrationConfiguration migrationConfiguration)
    {
        try
        {
            List<Script> unOrderedScripts;
          
            if (string.IsNullOrEmpty(value: migrationConfiguration.ScriptsDirectory))
            {
                unOrderedScripts = await _assemblyResourceHelper.TryGetScriptsFromAssembly(typeOfClassWhereScriptsAreLocated: typeOfClassWhereScriptsAreLocated).ConfigureAwait(continueOnCapturedContext: false);
            }
            else
            {
                unOrderedScripts = await _directoryHelper.TryGetScriptsFromDirectoryAsync(directory: migrationConfiguration.ScriptsDirectory).ConfigureAwait(continueOnCapturedContext: false);
            }

            _logger.Log(logLevel: LogLevel.Information, message: "Total scripts found: {unOrderedScripts.Count}", args: unOrderedScripts.Count);
            var unOrderedScriptsWithoutExcludedScripts = RemoveExcludedScripts(scripts: unOrderedScripts, excludedScripts: _excludedScriptsList);
            var orderedScriptsWithoutExcludedScripts = SetScriptsInCorrectSequence(scripts: unOrderedScriptsWithoutExcludedScripts);
            return new Result<List<Script>>(wasSuccessful: true, value: orderedScriptsWithoutExcludedScripts);
        }
        catch (Exception ex)
        {
            return new Result<List<Script>>(wasSuccessful: false, exception: ex);
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
                _logger.Log(logLevel: LogLevel.Warning, message: "script: {script.FileName} was skipped due to exception in previous script", args: script.FileName);
                continue;
            }

            var executedDateTime = _dataTimeHelper.GetCurrentUtcTime();

            var result = await _databaseConnector.RunDbMigrationScriptAsync(migrationConfiguration: migrationConfiguration
                , script: script
                , executedDateTime: executedDateTime
                , cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            if (result.Value == RunMigrationResult.MigrationWasCancelled)
            {
                _logger.Log(logLevel: LogLevel.Warning, message: $"migration process was canceled");
                return new Result<bool>(wasSuccessful: false);
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

        return skipBecauseOfErrorWithPreviousScript ? new Result<bool>(wasSuccessful: false) : new Result<bool>(wasSuccessful: true);
    }

    private static List<Script> RemoveExcludedScripts(List<Script> scripts, List<string> excludedScripts)
    {
        var result = scripts.Where(predicate: p => excludedScripts.All(predicate: x => x != p.FileName)).ToList();
        return result;
    }

    private static List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
    {
        return scripts.OrderBy(keySelector: s => s.DatePartOfName)
            .ThenBy(keySelector: s => s.SequenceNumberPart).ToList();
    }
}