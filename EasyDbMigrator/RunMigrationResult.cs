namespace EasyDbMigrator
{
    public enum RunMigrationResult
    {
        MigrationScriptExecuted,
        ScriptSkippedBecauseAlreadyRun,
        ExceptionWasThrownWhenScriptWasExecuted,
        MigrationWasCancelled,
    }
}
