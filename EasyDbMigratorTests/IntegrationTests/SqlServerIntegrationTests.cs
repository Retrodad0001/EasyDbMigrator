﻿// Ignore Spelling: Sql

using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.IntegrationTests.Helpers;
using EasyDbMigratorTests.TestHelpers;
using ExampleTestLibWithSqlServerScripts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Mssql;
using Xunit;

namespace EasyDbMigratorTests.IntegrationTests;

[ExcludeFromCodeCoverage]
[Collection(name: nameof(NotRunParallel))]
public class SqlServerIntegrationTests
{

    private const string DATABASE_NAME = "EasyDbMigratorSqlServer";

    [Fact]
    [Trait(name: "Category", value: "IntegrationTest")]
    public async Task When_nothing_goes_wrong_with_running_the_migrations_on_an_empty_database()
    {
        var dockerEnvironmentSql = SetupSqlDockerTestEnvironment();

        try
        {
            await dockerEnvironmentSql.UpAsync();
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(name: DATABASE_NAME)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString: connectionString ?? throw new InvalidOperationException()
                , databaseName: DATABASE_NAME);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: datetimeHelperMock.Object
                , databaseConnector: new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_CreateDB.sql"
            };

            migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededDeleteDatabase.Should().BeTrue();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereTheSqlServerScriptsCanBeFound)
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededRunningMigrations.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled(expectedMessage: "DeleteDatabaseIfExistAsync has executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211231_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(id: 1, executed: executedDataTime, filename: "20211230_002_Script2.sql", version: "1.0.0"),
                new DbMigrationsRunRowSqlServer(id: 2, executed: executedDataTime, filename: "20211231_001_Script1.sql", version: "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString: connectionString
              , expectedRows: expectedRows
              , testDatabaseName: DATABASE_NAME);

        }
        catch (Exception ex)
        {
            Assert.Fail(ex.ToString());
        }
        finally
        {
            await dockerEnvironmentSql.DisposeAsync();
        }
    }

    [Fact]
    [Trait(name: "Category", value: "IntegrationTest")]
    public async Task Can_skip_scripts_if_they_already_run_before()
    {
        var dockerEnvironmentSql = SetupSqlDockerTestEnvironment();

        try
        {
            await dockerEnvironmentSql.UpAsync();
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(name: DATABASE_NAME)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString: connectionString ?? throw new InvalidOperationException()
                , databaseName: DATABASE_NAME);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDataTimeHelper> datetimeHelperMock1 = new();
            var executedFirstTimeDataTime = DateTimeOffset.UtcNow;

            _ = datetimeHelperMock1.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedFirstTimeDataTime);

            var migrator1 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: datetimeHelperMock1.Object
                , databaseConnector: new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
                { "20211230_001_CreateDB.sql" };

            migrator1.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeededDeleteDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereTheSqlServerScriptsCanBeFound);

            bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeededRunningMigrations.Should().BeTrue();

            //now run the migrations again
            Mock<ILogger<DbMigrator>> loggerMockSecondRun = new();
            DateTime executedSecondTimeDataTime = new(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 1);

            Mock<IDataTimeHelper> datetimeHelperMock2 = new();
            _ = datetimeHelperMock2.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedSecondTimeDataTime);

            var migrator2 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
               , logger: loggerMockSecondRun.Object
               , dataTimeHelperMock: datetimeHelperMock2.Object
               , databaseConnector: new MicrosoftSqlConnector());

            migrator2.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeeded = await migrator2.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None);
            _ = succeeded.Should().BeTrue();

            _ = loggerMockSecondRun
                .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2.sql was not run because script was already executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "script: 20211231_001_Script1.sql was not run because script was already executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);

            //version - table should not be updated for the second time

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(id: 1, executed: executedFirstTimeDataTime, filename: "20211230_002_Script2.sql", version: "1.0.0"),
                new DbMigrationsRunRowSqlServer(id: 2, executed: executedFirstTimeDataTime, filename: "20211231_001_Script1.sql", version: "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString: connectionString
            , expectedRows: expectedRows
            , testDatabaseName: DATABASE_NAME);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.ToString());
        }
        finally
        {
            await dockerEnvironmentSql.DisposeAsync();
        }
    }

    [Fact]
    [Trait(name: "Category", value: "IntegrationTest")]
    public async Task Can_cancel_the_migration_process()
    {
        var dockerEnvironmentSql = SetupSqlDockerTestEnvironment();

        CancellationTokenSource source = new();
        var token = source.Token;

        try
        {
            await dockerEnvironmentSql.UpAsync(cancellationToken: token);
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(name: DATABASE_NAME)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString: connectionString ?? throw new InvalidOperationException()
                , databaseName: DATABASE_NAME);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: datetimeHelperMock.Object
                , databaseConnector: new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20212230_001_CreateDB.sql"
            };

            migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: token);
            _ = succeededDeleteDatabase.Should().BeTrue();

            source.Cancel();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereTheSqlServerScriptsCanBeFound)
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
            await dockerEnvironmentSql.DisposeAsync();
            source.Dispose();
        }
    }

    private static DockerEnvironment SetupSqlDockerTestEnvironment()
    {
        var environmentBuilder = new DockerEnvironmentBuilder();
        const string password = "stuffy6!";

        IDictionary<ushort, ushort> ports = new Dictionary<ushort, ushort>
        {
            { 1433, 1433 }
        };
        return (DockerEnvironment)environmentBuilder
             .SetName(environmentName: DATABASE_NAME)
             .AddMssqlContainer(paramsBuilder: p => p with
             {
                 Name = DATABASE_NAME,
                 SAPassword = password,
                 Ports = ports
             }).Build();
    }
}