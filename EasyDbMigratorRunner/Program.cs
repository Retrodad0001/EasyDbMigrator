using EasyDbMigrator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorRunner
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(/**string[] args**/)
        {
            ConsoleLoggerOptions options = new ConsoleLoggerOptions();
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                _ = builder.AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<DbMigrator>();
            MigrationConfiguration config = new MigrationConfiguration(connectionString: string.Empty
                , databaseName: string.Empty);
            DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config, logger: logger);
        }
    }
}
