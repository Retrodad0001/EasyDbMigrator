namespace EasyDbMigrator
{
    //TODO low: test add test check connection string correct format

    public class MigrationConfiguration
    {
        public string ConnectionString { get; }

        public string DatabaseName { get; }

        public MigrationConfiguration(string connectionString, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new System.ArgumentException("connectionString cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new System.ArgumentException($"'{nameof(databaseName)}' cannot be null or whitespace.", nameof(databaseName));
            }

            if (databaseName.Trim().Split(" ").Length > 1)
            {
                throw new System.ArgumentException($"'{nameof(databaseName)}' can only be one word.", nameof(databaseName));
            }

            ConnectionString = connectionString;
            DatabaseName = databaseName;
        }
    }
}
