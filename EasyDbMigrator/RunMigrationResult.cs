namespace EasyDbMigrator
{
    public enum RunMigrationResult
    {
        MigrationScriptExecuted,
        ScriptSkippedBecauseAlreadyRun,
        ExceptionWasThownWhenScriptWasExecuted,
    }

}
