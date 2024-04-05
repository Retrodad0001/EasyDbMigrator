namespace EasyDbMigrator;

/// <summary>
/// Represents the result of running a migration script.
/// </summary>
public enum RunMigrationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    MigrationScriptExecuted,
    ScriptSkippedBecauseAlreadyRun,
    ExceptionWasThrownWhenScriptWasExecuted,
    MigrationWasCancelled,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
