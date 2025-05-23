﻿// Ignore Spelling: Sql
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.IntegrationTests.Helpers;
using EasyDbMigratorTests.TestHelpers;
using ExampleTestLibWithSqlServerScripts;
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
    private const string DatabaseName = "EasyDbMigratorSqlServer";

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task WhenNothingGoesWrongWithRunningTheMigrationsOnAnEmptyDatabase()
    {
        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment();

        try
        {
            await dockerEnvironmentSql.UpAsync();

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(DatabaseName)?.GetConnectionString();
            System.Data.SqlClient.SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , DatabaseName);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDateTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            DbMigrator? migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new MicrosoftSqlConnector());

            List<string> list = new()
            {
                "20211230_001_CreateDB.sql"
            };
            List<string> scriptsToExclude = list;

            migrator.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None);
            Assert.True(succeededDeleteDatabase);

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None);
            Assert.True(succeededRunningMigrations);

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
              , DatabaseName);

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
    [Trait("Category", "IntegrationTest")]
    public async Task CanSkipScriptsIfTheyAlreadyRunBefore()
    {
        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment();

        try
        {
            await dockerEnvironmentSql.UpAsync();
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(DatabaseName)?.GetConnectionString();
            System.Data.SqlClient.SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , DatabaseName);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDateTimeHelper> datetimeHelperMock1 = new();
            DateTimeOffset executedFirstTimeDataTime = DateTimeOffset.UtcNow;

            _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(executedFirstTimeDataTime);

            DbMigrator? migrator1 = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock1.Object
                , new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
                { "20211230_001_CreateDB.sql" };

            migrator1.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None);
            Assert.True(succeededDeleteDatabase);

            Type? type = typeof(HereTheSqlServerScriptsCanBeFound);

            bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None);
            Assert.True(succeededRunningMigrations);

            //now run the migrations again
            Mock<ILogger<DbMigrator>> loggerMockSecondRun = new();
            DateTime executedSecondTimeDataTime = new(2021, 12, 31, 2, 16, 1);

            Mock<IDateTimeHelper> datetimeHelperMock2 = new();
            _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(executedSecondTimeDataTime);

            DbMigrator? migrator2 = DbMigrator.CreateForLocalIntegrationTesting(config
               , loggerMockSecondRun.Object
               , datetimeHelperMock2.Object
               , new MicrosoftSqlConnector());

            migrator2.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeeded = await migrator2.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None);
            Assert.True(succeeded);

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
            , DatabaseName);
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
    [Trait("Category", "IntegrationTest")]
    public async Task CanCancelTheMigrationProcess()
    {
        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment();

        CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        try
        {
            await dockerEnvironmentSql.UpAsync(token);
            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(DatabaseName)?.GetConnectionString();
            System.Data.SqlClient.SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , DatabaseName);

            Mock<ILogger<DbMigrator>> loggerMock = new();

            Mock<IDateTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            DbMigrator? migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new MicrosoftSqlConnector());

            List<string> scriptsToExclude = new()
            {
                "20212230_001_CreateDB.sql"
            };

            migrator.ExcludeTheseScriptsInRun(scriptsToExclude);

            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , token);
            Assert.True(succeededDeleteDatabase);

            source.Cancel();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , token);
            Assert.True(succeededRunningMigrations);

            _ = loggerMock
                .CheckIfLoggerWasCalled("migration process was canceled from the outside", LogLevel.Warning, Times.Exactly(1), false);
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
        DockerEnvironmentBuilder? environmentBuilder = new();
        const string password = "stuffy6!";

        IDictionary<ushort, ushort> ports = new Dictionary<ushort, ushort>
        {
            { 1433, 1433 }
        };
        return (DockerEnvironment)environmentBuilder
             .SetName(DatabaseName)
             .AddMssqlContainer(p => p with
             {
                 Name = DatabaseName,
                 SAPassword = password,
                 Ports = ports
             }).Build();
    }
}