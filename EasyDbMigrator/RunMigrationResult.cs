namespace EasyDbMigrator.Helpers
{
    public enum RunMigrationResult
    {
        MigrationScriptExecuted,
        ScriptSkippedBecauseAlreadyRun,
        ExceptionWasThownWhenScriptWasExecuted,
    }
}
