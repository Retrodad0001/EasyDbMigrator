namespace EasyDbMigrator;

public sealed record MigrationConfiguration
{
    public string ConnectionString { get; }
    public string DatabaseName { get; }
    public string? ScriptsDirectory { get; }

    public MigrationConfiguration(string connectionString, string databaseName, string? scriptsDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            throw new System.ArgumentException(message: "connectionString cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value: databaseName))
        {
            throw new System.ArgumentException(message: $"'{nameof(databaseName)}' cannot be null or whitespace.", paramName: nameof(databaseName));
        }

        CheckIfCorrectDatabaseName(databaseName: databaseName);

        ConnectionString = connectionString;
        DatabaseName = databaseName;
        ScriptsDirectory = scriptsDirectory;
    }

    private static void CheckIfCorrectDatabaseName(string databaseName)
    {
        if (databaseName.Trim().Split(separator: " ").Length > 1)
        {
            throw new System.ArgumentException(message: $"'{nameof(databaseName)}' can only be one word.", paramName: nameof(databaseName));
        }
    }
}
