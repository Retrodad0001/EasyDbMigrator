using EasyDbMigrator;
using EasyDbMigrator.Infra;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorRunner
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
#pragma warning disable CA1801 // Review unused parameters
        public static void Main(string[] args)
#pragma warning restore CA1801 // Review unused parameters
        {
            ConsoleLoggerOptions options = new();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                _ = builder.AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<DbMigrator>();

            loggerFactory.Dispose();

            MigrationConfiguration config = new MigrationConfiguration(connectionString: string.Empty, databaseName: string.Empty);

            DbMigrator migrator = new(logger: logger
                , migrationConfiguration: config
                , databaseconnector: new SqlDbConnector()
                , scriptsHelper: new AssemblyResourceHelper());
        }
    }
}
