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
[Collection(nameof(NotRunParallel))]
public class SqlServerIntegrationTests
{
    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task When_nothing_goes_wrong_with_running_the_migrations_on_an_empty_database()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerEnvironmentSql = SetupSqlDockerTestEnvironment(databaseName);

        try
        {
            await dockerEnvironmentSql.UpAsync().ConfigureAwait(true);
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20211230_001_CreateDB.sql"
            };

            migrator.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededDeleteDatabase.Should().BeTrue();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededRunningMigrations.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedDataTime, "20211230_002_Script2.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(2, executedDataTime, "20211231_001_Script1.sql", "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
              , expectedRows
              , databaseName);

        }
        catch (Exception ex)
        {
            Assert.True(false, ex.ToString());
        }
        finally
        {
            await dockerEnvironmentSql.DisposeAsync().ConfigureAwait(true);
        }
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task Can_skip_scripts_if_they_already_run_before()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerEnvironmentSql = SetupSqlDockerTestEnvironment(databaseName);

        try
        {
            await dockerEnvironmentSql.UpAsync().ConfigureAwait(true);
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDataTimeHelper> datetimeHelperMock1 = new();
            var executedFirstTimeDataTime = DateTimeOffset.UtcNow;

            _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(executedFirstTimeDataTime);

            var migrator1 = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock1.Object
                , new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
                { "20211230_001_CreateDB.sql" };

            migrator1.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededDeleteDatabase.Should().BeTrue();

            var type = typeof(HereTheSqlServerScriptsCanBeFound);

            bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeededRunningMigrations.Should().BeTrue();

            //now run the migrations again
            Mock<ILogger<DbMigrator>> loggerMockSecondRun = new();
            DateTime executedSecondTimeDataTime = new(2021, 12, 31, 2, 16, 1);

            Mock<IDataTimeHelper> datetimeHelperMock2 = new();
            _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(executedSecondTimeDataTime);

            var migrator2 = DbMigrator.CreateForLocalIntegrationTesting(config
               , loggerMockSecondRun.Object
               , datetimeHelperMock2.Object
               , new MicrosoftSqlConnector());

            migrator2.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeeded = await migrator2.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None).ConfigureAwait(true);
            _ = succeeded.Should().BeTrue();

            _ = loggerMockSecondRun
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);

            //version - table should not be updated for the second time

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedFirstTimeDataTime, "20211230_002_Script2.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(2, executedFirstTimeDataTime, "20211231_001_Script1.sql", "1.0.0")
            };

            _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
            , expectedRows
            , databaseName);
        }
        catch (Exception ex)
        {
            Assert.True(false, ex.ToString());
        }
        finally
        {
            await dockerEnvironmentSql.DisposeAsync().ConfigureAwait(true);
        }
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task Can_cancel_the_migration_process()
    {
        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        var dockerEnvironmentSql = SetupSqlDockerTestEnvironment(databaseName);

        CancellationTokenSource source = new();
        var token = source.Token;

        try
        {
            await dockerEnvironmentSql.UpAsync(token).ConfigureAwait(true);
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(databaseName)?.GetConnectionString();

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            var migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20212230_001_CreateDB.sql"
            };

            migrator.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , token).ConfigureAwait(true);
            _ = succeededDeleteDatabase.Should().BeTrue();

            source.Cancel();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
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
            await dockerEnvironmentSql.DisposeAsync().ConfigureAwait(true);
            source.Dispose();
        }
    }

    private static DockerEnvironment SetupSqlDockerTestEnvironment(string databaseName)
    {
        var environmentBuilder = new DockerEnvironmentBuilder();
        const string password = "stuffy6!";

        IDictionary<ushort, ushort> ports = new Dictionary<ushort, ushort>
        {
            { 1433, 1433 }
        };
        return (DockerEnvironment)environmentBuilder
             .SetName(databaseName)
             .AddMssqlContainer(p => p with
             {
                 Name = databaseName,
                 SAPassword = password,
                 Ports = ports
             }).Build();
    }
}