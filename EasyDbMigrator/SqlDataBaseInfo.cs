namespace EasyDbMigrator
{
    //TODO HIGH: add time-out policy or in configuration class?
    //TODO HIGH: add test check connection string correct format

    public class SqlDataBaseInfo //start with this no need for extra abstractions, use sql for now en sql only
    {
        public string ConnectionString { get; }

        public string DatabaseName { get; }

        public SqlDataBaseInfo(string connectionString, string databaseName)
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
