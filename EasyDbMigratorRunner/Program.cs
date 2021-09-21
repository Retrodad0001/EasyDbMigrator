using EasyDbMigrator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorRunner
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
#pragma warning disable CA1801 // Review unused parameters
#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1801 // Review unused parameters
        {
            ConsoleLoggerOptions options = new();

            var loggerFactory = LoggerFactory.Create(builder => {
                _ = builder.AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<DbMigrator>();

            loggerFactory.Dispose();
            DbMigrator migrator = new DbMigrator(logger: logger
                , sqlDbHelper: new EasyDbMigrator.Infra.SqlDbHelper()
                , scriptsHelper: new EasyDbMigrator.Infra.ScriptsHelper());
        }
    }
}
