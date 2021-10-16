using EasyDbMigrator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Diagnostics.CodeAnalysis;

//TODO replace with entity framework core
//TODO improving inner dev loop : create docker image in test
//TODO can use dbmigratior in own code with default .net dependency injection /start runner


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
