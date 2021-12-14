﻿namespace EasyDbMigrator
{
    public record MigrationConfiguration
    {
        public string ConnectionString { get; private set; }
        public string DatabaseName { get; private set; }
        public string? ScriptsDirectory { get; private set; }

        public MigrationConfiguration(string connectionString, string databaseName, string? ScriptsDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new System.ArgumentException("connectionString cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new System.ArgumentException($"'{nameof(databaseName)}' cannot be null or whitespace.", nameof(databaseName));
            }

            CheckIfCorrectDatabaseName(databaseName);

            ConnectionString = connectionString;
            DatabaseName = databaseName;
            this.ScriptsDirectory = ScriptsDirectory;
        }

        private static void CheckIfCorrectDatabaseName(string databaseName)
        {
            if (databaseName.Trim().Split(" ").Length > 1)
            {
                throw new System.ArgumentException($"'{nameof(databaseName)}' can only be one word.", nameof(databaseName));
            }
        }
    }
}
