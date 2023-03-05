using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.IntegrationTests.Helpers;
using EasyDbMigratorTests.TestHelpers;
using ExampleTestLibWithPostGreSQLServerScripts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Postgres;
using Xunit;

namespace EasyDbMigratorTests.IntegrationTests;

[ExcludeFromCodeCoverage]
[Collection(nameof(NotRunParallel))]
public class PostgresServerIntegrationTests
{
    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task When_nothing_goes_wrong_with_running_all_migrations_on_an_empty_database()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment(databaseName);

        try
        {
            await dockerPostgresServerEnvironment.UpAsync().ConfigureAwait(true);
            string connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTime executedDataTime = new(2021, 10, 17, 12, 10, 10);

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new PostgreSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_DoStuffScript.sql"
            };
            migrator.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededRunningMigrations.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2p.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1p.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);

            List<DbMigrationsRunRowPostgresServer> expectedRows = new()
            {
                new DbMigrationsRunRowPostgresServer(1, executedDataTime, "20211230_002_Script2p.sql", "1.0.0"),
                new DbMigrationsRunRowPostgresServer(2, executedDataTime, "20211231_001_Script1p.sql", "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTablePostgresSever(connectionString
              , expectedRows);

        }
        catch (Exception ex)
        {
            Assert.True(false, ex.ToString());
        }
        finally
        {
            await dockerPostgresServerEnvironment.DisposeAsync().ConfigureAwait(true);
        }
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task Can_skip_scripts_if_they_already_run_before()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment(databaseName);

        try
        {
            await dockerPostgresServerEnvironment.UpAsync().ConfigureAwait(true);
            string connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDataTimeHelper> datetimeHelperMock1 = new();
            DateTime executedFirstTimeDataTime = new(2021, 12, 30, 2, 16, 1);

            _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(executedFirstTimeDataTime);

            var migrator1 = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock1.Object
                , new PostgreSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_DoStuffScript.sql"
            };

            migrator1.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

            bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededRunningMigrations.Should().BeTrue();

            // now run the migrations again
            var loggerMockSecondRun = new Mock<ILogger<DbMigrator>>();
            DateTime executedSecondTimeDataTime = new(2021, 12, 31, 2, 16, 1);

            Mock<IDataTimeHelper> datetimeHelperMock2 = new();
            _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(executedSecondTimeDataTime);

            var migrator2 = DbMigrator.CreateForLocalIntegrationTesting(config
               , loggerMockSecondRun.Object
               , datetimeHelperMock2.Object
               , new PostgreSqlConnector());

            migrator2.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeeded = await migrator2.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeeded.Should().BeTrue();

            _ = loggerMockSecondRun
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2p.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1p.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);

            //  version - table should not be updated for the second time
            List<DbMigrationsRunRowPostgresServer> expectedRows = new()
            {
                new DbMigrationsRunRowPostgresServer(1, executedFirstTimeDataTime, "20211230_002_Script2p.sql", "1.0.0"),
                new DbMigrationsRunRowPostgresServer(2, executedFirstTimeDataTime, "20211231_001_Script1p.sql", "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTablePostgresSever(connectionString
             , expectedRows);

        }
        catch (Exception ex)
        {
            Assert.True(false, ex.ToString());
        }
        finally
        {
            await dockerPostgresServerEnvironment.DisposeAsync().ConfigureAwait(true);
        }
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task Can_cancel_the_migration_process()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment(databaseName);
        CancellationTokenSource source = new();
        var token = source.Token;

        try
        {
            await dockerPostgresServerEnvironment.UpAsync(token).ConfigureAwait(true);
            string? connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTime executedDataTime = new(2021, 10, 17, 12, 10, 10);

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new PostgreSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_DoStuffScript.sql"
            };
            migrator.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , token).ConfigureAwait(true);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

            source.Cancel();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(type
                , config
                , token).ConfigureAwait(true);
            _ = succeededRunningMigrations.Should().BeTrue();

            _ = loggerMock
                 .CheckIfLoggerWasCalled("migration process was canceled from the outside", LogLevel.Warning, Times.Exactly(1), false);
        }
        catch (Exception ex)
        {
            Assert.True(false, ex.ToString());
        }
        finally
        {
            source.Dispose();
            await dockerPostgresServerEnvironment.DisposeAsync().ConfigureAwait(true);
        }
    }

    private static DockerEnvironment SetupPostgresServerTestEnvironment(string databaseName)
    {
        var environmentBuilder = new DockerEnvironmentBuilder();

        const string userName = "retrodad";
        const string password = "stuffy6!";

        return (DockerEnvironment)environmentBuilder
             .SetName(databaseName)
             .AddPostgresContainer(p => p with
             {
                 Name = databaseName,
                 UserName = userName,
                 Password = password
             }).Build();
    }
}
