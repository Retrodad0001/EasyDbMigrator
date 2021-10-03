﻿using EasyDbMigrator;
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
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                _ = builder.AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<DbMigrator>();
            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_0_0
                , connectionString: string.Empty
                , databaseName: string.Empty);
            DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config, logger: logger);
        }
    }
}
