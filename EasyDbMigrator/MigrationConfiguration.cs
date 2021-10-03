namespace EasyDbMigrator
{

    public class MigrationConfiguration
    {
        public ApiVersion apiVersion { get; private set; }
        public string ConnectionString { get; private set; }
        public string DatabaseName { get; private set; }

        public MigrationConfiguration(ApiVersion apiVersion, string connectionString, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new System.ArgumentException("connectionString cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new System.ArgumentException($"'{nameof(databaseName)}' cannot be null or whitespace.", nameof(databaseName));
            }

            CheckCorrectDatabaseName(databaseName);

            this.apiVersion = apiVersion;
            ConnectionString = connectionString;
            DatabaseName = databaseName;
        }

        private static void CheckCorrectDatabaseName(string databaseName)
        {
            if (databaseName.Trim().Split(" ").Length > 1)
            {
                throw new System.ArgumentException($"'{nameof(databaseName)}' can only be one word.", nameof(databaseName));
            }
        }
    }
}
