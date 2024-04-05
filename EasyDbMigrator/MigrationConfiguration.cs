using System.Text;

namespace EasyDbMigrator;

/// <summary>
/// Represents the configuration for a migration.
/// </summary>
public readonly struct MigrationConfiguration
{
    /// <summary>
    /// The connection string to the database.
    /// </summary>
    public string ConnectionString { get; }
    /// <summary>
    /// The name of the database.
    /// </summary>
    public string DatabaseName { get; }
    /// <summary>
    /// The directory where the migration scripts are located.
    /// </summary>
    public string? ScriptsDirectory { get; }

    /// <summary>
    /// Constructor for a migration configuration.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="databaseName"></param>
    /// <param name="scriptsDirectory"></param>
    public MigrationConfiguration(string connectionString, string databaseName, string? scriptsDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new System.ArgumentException(
                new StringBuilder().Append('\'')
                    .Append(nameof(connectionString))
                    .Append("' cannot be null or whitespace.")
                    .ToString(), nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new System.ArgumentException(
                new StringBuilder().Append('\'')
                    .Append(nameof(databaseName))
                    .Append("' cannot be null or whitespace.")
                    .ToString(), nameof(databaseName));
        }

        CheckIfCorrectDatabaseName(databaseName);

        ConnectionString = connectionString;
        DatabaseName = databaseName;
        ScriptsDirectory = scriptsDirectory;
    }

    private static void CheckIfCorrectDatabaseName(string databaseName)
    {
        if (databaseName.Trim().Split(" ").Length > 1)
        {
            throw new System.ArgumentException(
                new StringBuilder().Append('\'')
                    .Append(nameof(databaseName))
                    .Append("' can only be one word.")
                    .ToString(), nameof(databaseName));
        }
    }
}
