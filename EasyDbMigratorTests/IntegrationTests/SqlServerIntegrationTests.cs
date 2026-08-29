// Ignore Spelling: Sql
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.IntegrationTests.Helpers;
using EasyDbMigratorTests.TestHelpers;
using ExampleTestLibWithSqlServerScripts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Mssql;

namespace EasyDbMigratorTests.IntegrationTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class SqlServerIntegrationTests
{
    /// <summary>Injected by MSTest; supplies the per-test cancellation token.</summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// The embedded example scripts open with "USE EasyDbMigratorSqlServer" and
    /// MicrosoftSqlConnector runs a script exactly as written, so the migration
    /// database has to be the one those scripts name. Every test still gets its
    /// own container under a random name, so this fixed name cannot collide with
    /// another test or another target framework.
    /// </summary>
    private const string DatabaseNameUsedByTheEmbeddedScripts = "EasyDbMigratorSqlServer";

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task WhenNothingGoesWrongWithRunningTheMigrationsOnAnEmptyDatabase()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string containerName = IntegrationTestHelper.GenerateRandomDatabaseName();
        const string databaseName = DatabaseNameUsedByTheEmbeddedScripts;
        diagnostics.Step("container name", containerName);
        diagnostics.Step("migration database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(containerName);

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(TestContext.CancellationToken);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(containerName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            Mock<ILogger<DbMigrator>> loggerMock = diagnostics.CreateLoggerMock<DbMigrator>("migration run");

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

            diagnostics.Step("dropping the database if it already exists");
            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededDeleteDatabase, "TryDeleteDatabaseIfExistAsync should succeed");

            diagnostics.Step("applying the migrations from the embedded resources");
            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededRunningMigrations, "TryApplyMigrationsAsync should succeed");
            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));

            diagnostics.Verify("the migrator logged the expected messages", () => _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false));

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedDataTime, "20211230_002_Script2.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(2, executedDataTime, "20211231_001_Script1.sql", "1.0.0")
            };

            diagnostics.Verify("the DbMigrationsRun table has the expected rows", () =>
                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
                    , expectedRows
                    , databaseName));
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();
        }
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task CanSkipScriptsIfTheyAlreadyRunBefore()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string containerName = IntegrationTestHelper.GenerateRandomDatabaseName();
        const string databaseName = DatabaseNameUsedByTheEmbeddedScripts;
        diagnostics.Step("container name", containerName);
        diagnostics.Step("migration database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(containerName);

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(TestContext.CancellationToken);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(containerName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            Mock<ILogger<DbMigrator>> loggerMock = diagnostics.CreateLoggerMock<DbMigrator>("first run");

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

            diagnostics.Step("dropping the database if it already exists");
            bool succeededDeleteDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededDeleteDatabase, "TryDeleteDatabaseIfExistAsync should succeed");

            Type? type = typeof(HereTheSqlServerScriptsCanBeFound);

            diagnostics.Step("first migration run: all scripts should be applied");
            bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededRunningMigrations, "the first TryApplyMigrationsAsync should succeed");
            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));

            Mock<ILogger<DbMigrator>> loggerMockSecondRun = diagnostics.CreateLoggerMock<DbMigrator>("second run");
            DateTime executedSecondTimeDataTime = new(2021, 12, 31, 2, 16, 1);

            Mock<IDateTimeHelper> datetimeHelperMock2 = new();
            _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(executedSecondTimeDataTime);

            DbMigrator? migrator2 = DbMigrator.CreateForLocalIntegrationTesting(config
               , loggerMockSecondRun.Object
               , datetimeHelperMock2.Object
               , new MicrosoftSqlConnector());

            migrator2.ExcludeTheseScriptsInRun(scriptsToExclude);

            diagnostics.Step("second migration run: every script should be skipped");
            bool succeeded = await migrator2.TryApplyMigrationsAsync(type
                , config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeeded, "the second TryApplyMigrationsAsync should succeed");

            diagnostics.Verify("the second run logged that both scripts were skipped", () => _ = loggerMockSecondRun
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false));

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedFirstTimeDataTime, "20211230_002_Script2.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(2, executedFirstTimeDataTime, "20211231_001_Script1.sql", "1.0.0")
            };

            diagnostics.Verify("the DbMigrationsRun table still holds the timestamps of the first run", () =>
                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
                    , expectedRows
                    , databaseName));
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();
        }
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task CanCancelTheMigrationProcess()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string containerName = IntegrationTestHelper.GenerateRandomDatabaseName();
        const string databaseName = DatabaseNameUsedByTheEmbeddedScripts;
        diagnostics.Step("container name", containerName);
        diagnostics.Step("migration database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(containerName);

        CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(token);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(containerName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            Mock<ILogger<DbMigrator>> loggerMock = diagnostics.CreateLoggerMock<DbMigrator>("cancelled run");

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

            diagnostics.Step("dropping the database if it already exists");
            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , token);
            diagnostics.AssertIsTrue(succeededDeleteDatabase, "TryDeleteDatabaseIfExistAsync should succeed");

            diagnostics.Step("cancelling the token before the migrations start");
            source.Cancel();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , token);
            diagnostics.AssertIsTrue(succeededRunningMigrations, "a cancelled run should report success (nothing was done)");

            diagnostics.Verify("the migrator logged that the run was cancelled", () => _ = loggerMock
                .CheckIfLoggerWasCalled("migration process was canceled from the outside", LogLevel.Warning, Times.Exactly(1), false));
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();
            source.Dispose();
        }
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task WhenScriptsComeFromADirectoryTheMigrationsAreApplied()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        diagnostics.Step("generated database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(databaseName);

        string scriptsDirectory = Path.Combine(Path.GetTempPath(), databaseName);
        _ = Directory.CreateDirectory(scriptsDirectory);
        diagnostics.Step("scripts directory", scriptsDirectory);

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(TestContext.CancellationToken);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(databaseName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20211230_002_Script2.sql")
                , "CREATE TABLE Customers (Id INT NOT NULL);", TestContext.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20211231_001_Script1.sql")
                , "CREATE TABLE Orders (Id INT NOT NULL);", TestContext.CancellationToken);
            diagnostics.Step("wrote 2 migration scripts to the scripts directory");

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName
                , scriptsDirectory);

            Mock<ILogger<DbMigrator>> loggerMock = diagnostics.CreateLoggerMock<DbMigrator>("migration run");

            Mock<IDateTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            DbMigrator? migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new MicrosoftSqlConnector());

            diagnostics.Step("dropping the database if it already exists");
            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededDeleteDatabase, "TryDeleteDatabaseIfExistAsync should succeed");

            diagnostics.Step("applying the migrations from the scripts directory");
            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededRunningMigrations, "TryApplyMigrationsAsync should succeed");
            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));

            diagnostics.Verify("the migrator logged that both scripts ran", () => _ = loggerMock
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false));

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedDataTime, "20211230_002_Script2.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(2, executedDataTime, "20211231_001_Script1.sql", "1.0.0")
            };

            diagnostics.Verify("the DbMigrationsRun table has the expected rows", () =>
                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
                    , expectedRows
                    , databaseName));
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();

            if (Directory.Exists(scriptsDirectory))
            {
                Directory.Delete(scriptsDirectory, true);
            }
        }
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task WhenAScriptFailsTheRunStopsAndLaterScriptsAreSkipped()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        diagnostics.Step("generated database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(databaseName);

        string scriptsDirectory = Path.Combine(Path.GetTempPath(), databaseName);
        _ = Directory.CreateDirectory(scriptsDirectory);
        diagnostics.Step("scripts directory", scriptsDirectory);

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(TestContext.CancellationToken);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(databaseName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20220101_001_CreateCustomers.sql")
                , "CREATE TABLE Customers (Id INT NOT NULL);", TestContext.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20220101_002_Broken.sql")
                , "INSERT INTO ThisTableDoesNotExist (Id) VALUES (1);", TestContext.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20220102_001_CreateOrders.sql")
                , "CREATE TABLE Orders (Id INT NOT NULL);", TestContext.CancellationToken);
            diagnostics.Step("wrote 3 migration scripts, the middle one is deliberately broken");

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName
                , scriptsDirectory);

            Mock<ILogger<DbMigrator>> loggerMock = diagnostics.CreateLoggerMock<DbMigrator>("migration run");

            Mock<IDateTimeHelper> datetimeHelperMock = new();
            DateTimeOffset executedDataTime = DateTime.UtcNow;

            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            DbMigrator? migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , datetimeHelperMock.Object
                , new MicrosoftSqlConnector());

            diagnostics.Step("dropping the database if it already exists");
            bool succeededDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededDeleteDatabase, "TryDeleteDatabaseIfExistAsync should succeed");

            diagnostics.Step("applying the migrations, the run is expected to fail on the broken script");
            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None);
            diagnostics.AssertIsFalse(succeededRunningMigrations, "TryApplyMigrationsAsync should report the failed script");
            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));

            diagnostics.Verify("the migrator logged the failure and the skipped script", () => _ = loggerMock
                .CheckIfLoggerWasCalled("script: 20220101_001_CreateCustomers.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20220101_002_Broken.sql was not completed due to exception", LogLevel.Error, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20220102_001_CreateOrders.sql was skipped due to exception in previous script", LogLevel.Warning, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false));

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedDataTime, "20220101_001_CreateCustomers.sql", "1.0.0")
            };

            diagnostics.Verify("only the successful script is recorded in DbMigrationsRun", () =>
                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
                    , expectedRows
                    , databaseName));

            diagnostics.AssertIsTrue(IntegrationTestHelper.TableExistsSqlServer(connectionString, databaseName, "Customers")
                , "the Customers table from the first script should exist");
            diagnostics.AssertIsFalse(IntegrationTestHelper.TableExistsSqlServer(connectionString, databaseName, "Orders")
                , "the Orders table from the skipped script should not exist");
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();

            if (Directory.Exists(scriptsDirectory))
            {
                Directory.Delete(scriptsDirectory, true);
            }
        }
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task TryDeleteDatabaseIfExistAsyncReallyDropsTheDatabase()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string containerName = IntegrationTestHelper.GenerateRandomDatabaseName();
        const string databaseName = DatabaseNameUsedByTheEmbeddedScripts;
        diagnostics.Step("container name", containerName);
        diagnostics.Step("migration database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(containerName);

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(TestContext.CancellationToken);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(containerName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            List<string> scriptsToExclude = new()
            {
                "20211230_001_CreateDB.sql"
            };

            Mock<ILogger<DbMigrator>> loggerMockFirstRun = diagnostics.CreateLoggerMock<DbMigrator>("first run");
            Mock<IDateTimeHelper> datetimeHelperMock1 = new();
            DateTimeOffset executedFirstTimeDataTime = DateTime.UtcNow;
            _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(executedFirstTimeDataTime);

            DbMigrator? migrator1 = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMockFirstRun.Object
                , datetimeHelperMock1.Object
                , new MicrosoftSqlConnector());

            migrator1.ExcludeTheseScriptsInRun(scriptsToExclude);

            diagnostics.Step("dropping the database if it already exists");
            diagnostics.AssertIsTrue(await migrator1.TryDeleteDatabaseIfExistAsync(config, CancellationToken.None)
                , "the first TryDeleteDatabaseIfExistAsync should succeed");

            diagnostics.Step("first migration run: creates the database and the versioning table");
            diagnostics.AssertIsTrue(await migrator1.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None)
                , "the first TryApplyMigrationsAsync should succeed");
            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));

            diagnostics.AssertIsTrue(IntegrationTestHelper.DatabaseExistsSqlServer(connectionString, databaseName)
                , "the database should exist after the first run");
            diagnostics.AssertIsTrue(IntegrationTestHelper.TableExistsSqlServer(connectionString, databaseName, "DbMigrationsRun")
                , "the DbMigrationsRun table should exist after the first run");

            diagnostics.Step("dropping the database that was just migrated");
            bool succeededDeleteDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None);
            diagnostics.AssertIsTrue(succeededDeleteDatabase, "the second TryDeleteDatabaseIfExistAsync should succeed");

            diagnostics.AssertIsFalse(IntegrationTestHelper.DatabaseExistsSqlServer(connectionString, databaseName)
                , "the database should be gone after it was dropped");

            Mock<ILogger<DbMigrator>> loggerMockSecondRun = diagnostics.CreateLoggerMock<DbMigrator>("second run");
            Mock<IDateTimeHelper> datetimeHelperMock2 = new();
            DateTimeOffset executedSecondTimeDataTime = new DateTime(2022, 1, 5, 8, 30, 0);
            _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(executedSecondTimeDataTime);

            DbMigrator? migrator2 = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMockSecondRun.Object
                , datetimeHelperMock2.Object
                , new MicrosoftSqlConnector());

            migrator2.ExcludeTheseScriptsInRun(scriptsToExclude);

            diagnostics.Step("second migration run: should start from scratch on the recreated database");
            diagnostics.AssertIsTrue(await migrator2.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None)
                , "the second TryApplyMigrationsAsync should succeed");

            diagnostics.Verify("the second run logged that both scripts ran again", () => _ = loggerMockSecondRun
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false));

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedSecondTimeDataTime, "20211230_002_Script2.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(2, executedSecondTimeDataTime, "20211231_001_Script1.sql", "1.0.0")
            };

            diagnostics.Verify("DbMigrationsRun restarted at Id 1 with the new timestamps", () =>
                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
                    , expectedRows
                    , databaseName));
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();
        }
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ASecondRunAppliesOnlyTheNewlyAddedScript()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string databaseName = IntegrationTestHelper.GenerateRandomDatabaseName();
        diagnostics.Step("generated database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(databaseName);

        string scriptsDirectory = Path.Combine(Path.GetTempPath(), databaseName);
        _ = Directory.CreateDirectory(scriptsDirectory);
        diagnostics.Step("scripts directory", scriptsDirectory);

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(TestContext.CancellationToken);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(databaseName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName
                , scriptsDirectory);

            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20220101_001_CreateCustomers.sql")
                , "CREATE TABLE Customers (Id INT NOT NULL);", TestContext.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20220102_001_CreateOrders.sql")
                , "CREATE TABLE Orders (Id INT NOT NULL);", TestContext.CancellationToken);
            diagnostics.Step("wrote the 2 scripts of the first release");

            Mock<ILogger<DbMigrator>> loggerMockFirstRun = diagnostics.CreateLoggerMock<DbMigrator>("first run");
            Mock<IDateTimeHelper> datetimeHelperMock1 = new();
            DateTimeOffset executedFirstTimeDataTime = new DateTime(2022, 1, 10, 9, 0, 0);
            _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(executedFirstTimeDataTime);

            DbMigrator? migrator1 = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMockFirstRun.Object
                , datetimeHelperMock1.Object
                , new MicrosoftSqlConnector());

            diagnostics.Step("dropping the database if it already exists");
            diagnostics.AssertIsTrue(await migrator1.TryDeleteDatabaseIfExistAsync(config, CancellationToken.None)
                , "TryDeleteDatabaseIfExistAsync should succeed");

            diagnostics.Step("first migration run: applies the 2 scripts of the first release");
            diagnostics.AssertIsTrue(await migrator1.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None)
                , "the first TryApplyMigrationsAsync should succeed");
            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));

            await File.WriteAllTextAsync(Path.Combine(scriptsDirectory, "20220103_001_CreateInvoices.sql")
                , "CREATE TABLE Invoices (Id INT NOT NULL);", TestContext.CancellationToken);
            diagnostics.Step("wrote the extra script that arrives with the second release");

            Mock<ILogger<DbMigrator>> loggerMockSecondRun = diagnostics.CreateLoggerMock<DbMigrator>("second run");
            Mock<IDateTimeHelper> datetimeHelperMock2 = new();
            DateTimeOffset executedSecondTimeDataTime = new DateTime(2022, 2, 20, 14, 45, 0);
            _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(executedSecondTimeDataTime);

            DbMigrator? migrator2 = DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMockSecondRun.Object
                , datetimeHelperMock2.Object
                , new MicrosoftSqlConnector());

            diagnostics.Step("second migration run: only the new script should be applied");
            diagnostics.AssertIsTrue(await migrator2.TryApplyMigrationsAsync(typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , CancellationToken.None)
                , "the second TryApplyMigrationsAsync should succeed");
            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));

            diagnostics.Verify("the second run skipped the old scripts and ran only the new one", () => _ = loggerMockSecondRun
                .CheckIfLoggerWasCalled("script: 20220101_001_CreateCustomers.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20220102_001_CreateOrders.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20220103_001_CreateInvoices.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false));

            List<DbMigrationsRunRowSqlServer> expectedRows = new()
            {
                new DbMigrationsRunRowSqlServer(1, executedFirstTimeDataTime, "20220101_001_CreateCustomers.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(2, executedFirstTimeDataTime, "20220102_001_CreateOrders.sql", "1.0.0"),
                new DbMigrationsRunRowSqlServer(3, executedSecondTimeDataTime, "20220103_001_CreateInvoices.sql", "1.0.0")
            };

            diagnostics.Verify("only the third row of DbMigrationsRun carries the second timestamp", () =>
                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString
                    , expectedRows
                    , databaseName));

            diagnostics.AssertIsTrue(IntegrationTestHelper.TableExistsSqlServer(connectionString, databaseName, "Invoices")
                , "the Invoices table from the new script should exist");
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();

            if (Directory.Exists(scriptsDirectory))
            {
                Directory.Delete(scriptsDirectory, true);
            }
        }
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task AConsumerCanSupplyTheirOwnLogger()
    {
        TestDiagnostics diagnostics = new(TestContext);

        string containerName = IntegrationTestHelper.GenerateRandomDatabaseName();
        const string databaseName = DatabaseNameUsedByTheEmbeddedScripts;
        diagnostics.Step("container name", containerName);
        diagnostics.Step("migration database name", databaseName);

        DockerEnvironment? dockerEnvironmentSql = SetupSqlDockerTestEnvironment(containerName);

        try
        {
            diagnostics.Step("starting the SQL Server container (this pulls the image on a cold machine)");
            await dockerEnvironmentSql.UpAsync(TestContext.CancellationToken);
            diagnostics.Step("SQL Server container is up");

            string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(containerName)?.GetConnectionString();
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
            diagnostics.Step("connection string from the container", connectionString);

            MigrationConfiguration config = new(connectionString ?? throw new InvalidOperationException()
                , databaseName);

            List<string> scriptsToExclude = new()
            {
                "20211230_001_CreateDB.sql"
            };

            ConsumerSuppliedLogger<DbMigrator> firstRunLogger = new();

            DbMigrator migratorWithConsumerLogger = DbMigrator.Create(config
                , firstRunLogger
                , new MicrosoftSqlConnector());

            migratorWithConsumerLogger.ExcludeTheseScriptsInRun(scriptsToExclude);

            diagnostics.Step("applying the migrations without ever asking for the database to be dropped");
            bool succeededFirstUpdate = await migratorWithConsumerLogger.TryApplyMigrationsAsync(
                typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , TestContext.CancellationToken);

            diagnostics.AssertIsTrue(succeededFirstUpdate
                , new StringBuilder()
                    .Append("the migration run should succeed with a consumer supplied logger, it logged:")
                    .Append(Environment.NewLine)
                    .Append(firstRunLogger.Describe())
                    .ToString());

            diagnostics.Step(IntegrationTestHelper.DescribeTablesSqlServer(connectionString, databaseName));
            diagnostics.Step("consumer logger received", firstRunLogger.Entries.Count);

            diagnostics.AssertIsTrue(firstRunLogger.Received(LogLevel.Information, "script: 20211230_002_Script2.sql was run")
                , "the consumer logger should have been told that the first script ran");
            diagnostics.AssertIsTrue(firstRunLogger.Received(LogLevel.Information, "script: 20211231_001_Script1.sql was run")
                , "the consumer logger should have been told that the second script ran");
            diagnostics.AssertIsTrue(firstRunLogger.Received(LogLevel.Information, "migration process executed successfully")
                , "the consumer logger should have been told that the migration process succeeded");

            List<DbMigrationsRunRowSqlServer> rowsAfterFirstUpdate =
                IntegrationTestHelper.ReadMigrationsRunTableSqlServer(connectionString, databaseName);
            diagnostics.Step(IntegrationTestHelper.DescribeRows(rowsAfterFirstUpdate));

            diagnostics.AssertIsTrue(rowsAfterFirstUpdate.Count == 2
                , "DbMigrationsRun should hold one row per applied script");

            ConsumerSuppliedLogger<DbMigrator> secondRunLogger = new();

            DbMigrator migratorForSecondUpdate = DbMigrator.Create(config
                , secondRunLogger
                , new MicrosoftSqlConnector());

            migratorForSecondUpdate.ExcludeTheseScriptsInRun(scriptsToExclude);

            diagnostics.Step("updating the same database a second time, nothing new should be applied");
            bool succeededSecondUpdate = await migratorForSecondUpdate.TryApplyMigrationsAsync(
                typeof(HereTheSqlServerScriptsCanBeFound)
                , config
                , TestContext.CancellationToken);

            diagnostics.AssertIsTrue(succeededSecondUpdate
                , new StringBuilder()
                    .Append("the second update should succeed, it logged:")
                    .Append(Environment.NewLine)
                    .Append(secondRunLogger.Describe())
                    .ToString());

            diagnostics.AssertIsTrue(secondRunLogger.Received(LogLevel.Information, "script: 20211230_002_Script2.sql was not run because script was already executed")
                , "the consumer logger should have been told that the first script was skipped");
            diagnostics.AssertIsTrue(secondRunLogger.Received(LogLevel.Information, "script: 20211231_001_Script1.sql was not run because script was already executed")
                , "the consumer logger should have been told that the second script was skipped");

            List<DbMigrationsRunRowSqlServer> rowsAfterSecondUpdate =
                IntegrationTestHelper.ReadMigrationsRunTableSqlServer(connectionString, databaseName);
            diagnostics.Step(IntegrationTestHelper.DescribeRows(rowsAfterSecondUpdate));

            diagnostics.Verify("the second update left the existing rows exactly as they were", () =>
                Assert.AreSequenceEqual(rowsAfterFirstUpdate, rowsAfterSecondUpdate));
        }
        catch (Exception ex)
        {
            Assert.Fail(diagnostics.Describe(ex));
        }
        finally
        {
            diagnostics.Step("disposing the SQL Server container");
            await dockerEnvironmentSql.DisposeAsync();
        }
    }

    private static DockerEnvironment SetupSqlDockerTestEnvironment(string databaseName)
    {
        DockerEnvironmentBuilder? environmentBuilder = new();
        const string password = "stuffy6!";

        return (DockerEnvironment)environmentBuilder
             .SetName(databaseName)
             .AddMssqlContainer(p => p with
             {
                 Name = databaseName,
                 SAPassword = password
             }).Build();
    }
}
