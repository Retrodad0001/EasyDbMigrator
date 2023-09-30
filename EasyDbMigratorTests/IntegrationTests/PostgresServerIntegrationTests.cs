// Ignore Spelling: postgres

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
[Collection(name: nameof(NotRunParallel))]
public class PostgresServerIntegrationTests
{
    [Fact]
    [Trait(name: "Category", value: "IntegrationTest")]
    public async Task When_nothing_goes_wrong_with_running_all_postgres_migrations_on_an_empty_database()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment(databaseName: databaseName);

        try
        {
            await dockerPostgresServerEnvironment.UpAsync();
            string connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(name: databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString: connectionString ?? throw new InvalidOperationException()
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTime executedDataTime = new(year: 2021, month: 10, day: 17, hour: 12, minute: 10, second: 10);

            _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: datetimeHelperMock.Object
                , databaseConnector: new PostgreSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_DoStuffScript.sql"
            };
            migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededRunningMigrations.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled(expectedMessage: "DeleteDatabaseIfExistAsync has executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2p.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211231_001_Script1p.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);

            List<DbMigrationsRunRowPostgresServer> expectedRows = new()
            {
                new DbMigrationsRunRowPostgresServer(id: 1, executed: executedDataTime, filename: "20211230_002_Script2p.sql", version: "1.0.0"),
                new DbMigrationsRunRowPostgresServer(id: 2, executed: executedDataTime, filename: "20211231_001_Script1p.sql", version: "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTablePostgresSever(connectionString: connectionString
              , expectedRows: expectedRows);

        }
        catch (Exception ex)
        {
            Assert.Fail(ex.ToString());
        }
        finally
        {
            await dockerPostgresServerEnvironment.DisposeAsync();
        }
    }

    [Fact]
    [Trait(name: "Category", value: "IntegrationTest")]
    public async Task Can_skip_postgres_scripts_if_they_already_run_before()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment(databaseName: databaseName);

        try
        {
            await dockerPostgresServerEnvironment.UpAsync();
            string connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(name: databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString: connectionString ?? throw new InvalidOperationException()
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDataTimeHelper> datetimeHelperMock1 = new();
            DateTime executedFirstTimeDataTime = new(year: 2021, month: 12, day: 30, hour: 2, minute: 16, second: 1);

            _ = datetimeHelperMock1.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedFirstTimeDataTime);

            var migrator1 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: datetimeHelperMock1.Object
                , databaseConnector: new PostgreSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_DoStuffScript.sql"
            };

            migrator1.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeededDeleteDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

            bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededRunningMigrations.Should().BeTrue();

            // now run the migrations again
            var loggerMockSecondRun = new Mock<ILogger<DbMigrator>>();
            DateTime executedSecondTimeDataTime = new(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 1);

            Mock<IDataTimeHelper> datetimeHelperMock2 = new();
            _ = datetimeHelperMock2.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedSecondTimeDataTime);

            var migrator2 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
               , logger: loggerMockSecondRun.Object
               , dataTimeHelperMock: datetimeHelperMock2.Object
               , databaseConnector: new PostgreSqlConnector());

            migrator2.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeeded = await migrator2.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeeded.Should().BeTrue();

            _ = loggerMockSecondRun
                .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2p.sql was not run because script was already executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211231_001_Script1p.sql was not run because script was already executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);

            //  version - table should not be updated for the second time
            List<DbMigrationsRunRowPostgresServer> expectedRows = new()
            {
                new DbMigrationsRunRowPostgresServer(id: 1, executed: executedFirstTimeDataTime, filename: "20211230_002_Script2p.sql", version: "1.0.0"),
                new DbMigrationsRunRowPostgresServer(id: 2, executed: executedFirstTimeDataTime, filename: "20211231_001_Script1p.sql", version: "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTablePostgresSever(connectionString: connectionString
             , expectedRows: expectedRows);

        }
        catch (Exception ex)
        {
            Assert.Fail(ex.ToString());
        }
        finally
        {
            await dockerPostgresServerEnvironment.DisposeAsync();
        }
    }

    [Fact]
    [Trait(name: "Category", value: "IntegrationTest")]
    public async Task Can_cancel_the_postgres_migration_process()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment(databaseName: databaseName);
        CancellationTokenSource source = new();
        var token = source.Token;

        try
        {
            await dockerPostgresServerEnvironment.UpAsync(cancellationToken: token);
            string? connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(name: databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString: connectionString ?? throw new InvalidOperationException()
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTime executedDataTime = new(year: 2021, month: 10, day: 17, hour: 12, minute: 10, second: 10);

            _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: datetimeHelperMock.Object
                , databaseConnector: new PostgreSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_DoStuffScript.sql"
            };
            migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: token);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

            source.Cancel();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                , migrationConfiguration: config
                , cancellationToken: token);
            _ = succeededRunningMigrations.Should().BeTrue();

            _ = loggerMock
                 .CheckIfLoggerWasCalled(expectedMessage: "migration process was canceled from the outside", expectedLogLevel: LogLevel.Warning, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.ToString());
        }
        finally
        {
            source.Dispose();
            await dockerPostgresServerEnvironment.DisposeAsync();
        }
    }

    private static DockerEnvironment SetupPostgresServerTestEnvironment(string databaseName)
    {
        var environmentBuilder = new DockerEnvironmentBuilder();

        const string userName = "retrodad";
        const string password = "stuffy6!";

        return (DockerEnvironment)environmentBuilder
             .SetName(environmentName: databaseName)
             .AddPostgresContainer(paramsBuilder: p => p with
             {
                 Name = databaseName,
                 UserName = userName,
                 Password = password
             }).Build();
    }
}
