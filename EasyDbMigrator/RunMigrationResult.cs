namespace EasyDbMigrator
{
    public enum RunMigrationResult
    {
        MigrationScriptExecuted,
        ScriptSkippedBecauseAllreadyRun,
        ExceptionWasThrownWhenScriptWasExecuted,
        MigrationWasCancelled,
    }
}
