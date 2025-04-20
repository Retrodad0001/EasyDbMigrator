// Ignore Spelling: versioning Migrator
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
public class DbMigratorTests
{
    [Fact]
    public void WhenConstructingTheParameterDirectoryHelperShouldBeProvided()
    {
        static void act()
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDateTimeHelper> datetimeHelperMock = new();

            DbMigrator unused = new(loggerMock.Object
                , new MicrosoftSqlConnector()
                , new AssemblyResourceHelper()
                , null!
                , datetimeHelperMock.Object);
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenConstructingTheParameterConnectorShouldBeProvided()
    {
        static void act()
        {

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDateTimeHelper> datetimeHelperMock = new();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new(loggerMock.Object
                , null!
                , new AssemblyResourceHelper()
                , directoryHelper
                , datetimeHelperMock.Object);
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenConstructingTheParameterAssemblyResourceHelperShouldBeProvided()
    {
        static void act()
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDateTimeHelper> datetimeHelperMock = new();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new(loggerMock.Object
                , new MicrosoftSqlConnector()
                , null!
                , directoryHelper
                , datetimeHelperMock.Object);
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenConstructingTheParameterLoggerShouldBeProvided()
    {
        static void act()
        {
            Mock<IDateTimeHelper> datetimeHelperMock = new();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new(null!
                , new MicrosoftSqlConnector()
                , new AssemblyResourceHelper()
                , directoryHelper
                , datetimeHelperMock.Object);
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenConstructingTheParameterDataTimeHelperShouldBeProvided()
    {
        static void act()
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new(loggerMock.Object
                , new MicrosoftSqlConnector()
                , new AssemblyResourceHelper()
                , directoryHelper
                , null!);
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenUsingTheCreateMethodHappyFlow()
    {
        static void act()
        {
            MigrationConfiguration config = new("connection string"
                , "databaseName");
            var loggerMock = new Mock<ILogger<DbMigrator>>();

            _ = DbMigrator.Create(config
                , loggerMock.Object
                , new MicrosoftSqlConnector());
        }

        act();
    }

    [Fact]
    public void WhenUsingTheCreateMethodTheILoggerShouldBeProvided()
    {
        static void act()
        {
            MigrationConfiguration config = new("connection string"
                , "databaseName");
            DbMigrator.Create(config
                , null!
                , new PostgreSqlConnector());
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenUsingTheCreateMethodTheMigrationConfigurationShouldBeProvided()
    {
        static void act()
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            DbMigrator? unused = DbMigrator.Create(null!
                , loggerMock.Object
                , new MicrosoftSqlConnector());
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenUsingTheCreateMethodTheIDatabaseConnectorShouldBeProvided()
    {
        static void act()
        {
            MigrationConfiguration config = new("connection string"
              , "databaseName");
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            DbMigrator? unused = DbMigrator.Create(config
                , loggerMock.Object
                , null!);
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenUsingTheCreateForLocalIntegrationTestingMethodHappyFlow()
    {
        static void act()
        {
            MigrationConfiguration config = new("connection string"
                , "databaseName");
            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDateTimeHelper> dataTimeHelperMock = new();
            DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , dataTimeHelperMock.Object
                , new MicrosoftSqlConnector());
        }

        act();
    }

    [Fact]
    public void WhenUsingTheCreateForLocalIntegrationTestingMethodTheILoggerShouldBeProvided()
    {
        static void act()
        {
            MigrationConfiguration config = new("connection string"
                , "databaseName");
            Mock<IDateTimeHelper> dataTimeHelperMock = new();

            DbMigrator? unused = DbMigrator.CreateForLocalIntegrationTesting(config
                , null!
                , dataTimeHelperMock.Object
                , new MicrosoftSqlConnector());
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenUsingTheCreateForLocalIntegrationTestingMethodTheMigrationConfigurationShouldBeProvided()
    {
        static void act()
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDateTimeHelper> dataTimeHelperMock = new();
            DbMigrator.CreateForLocalIntegrationTesting(null!
                , loggerMock.Object
                , dataTimeHelperMock.Object
                , new PostgreSqlConnector());
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenUsingTheCreateForLocalIntegrationTestingMethodTheDataTimeHelperShouldBeProvided()
    {
        static void act()
        {
            MigrationConfiguration config = new("connection string"
                , "databaseName");

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , null!
                , new MicrosoftSqlConnector());
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public void WhenUsingTheCreateForLocalIntegrationTestingMethodTheIDateBaseConnectorShouldBeProvided()
    {
        static void act()
        {
            MigrationConfiguration config = new("connection string"
                , "databasename");
            Mock<IDateTimeHelper> dataTimeHelperMock = new();

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            DbMigrator.CreateForLocalIntegrationTesting(config
                , loggerMock.Object
                , dataTimeHelperMock.Object
                , null!);
        }

        Assert.Throws<ArgumentNullException>(() => act());
    }

    [Fact]
    public async Task WhenMigrationProcessGoesOk()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connectionString";

        MigrationConfiguration config = new(connectionString
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        Result<RunMigrationResult> resultRunMigrations = new(true);
        _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , CancellationToken.None);

        Assert.True(result);

        _ = loggerMock
            .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled(
                new StringBuilder().Append("connection-string used: ").Append(connectionString).ToString(), LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);
    }

    [Fact]
    public async Task CanUseFileDirectoryForScripts()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connectionString";

        MigrationConfiguration config = new(connectionString
            , databaseName
            , "some directory");

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new("20211230_001_Scripta.sql", "some content");
        Script script2 = new("20211230_002_Scriptb.sql", "some content");

        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

        var directoryHelperMock = new Mock<IDirectoryHelper>();
        _ = directoryHelperMock.Setup(m => m.TryGetScriptsFromDirectoryAsync(It.IsAny<string>())).ReturnsAsync(() => scripts);

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        Result<RunMigrationResult> resultRunMigrations = new(true);
        _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , CancellationToken.None);

        Assert.True(result);

        _ = loggerMock
            .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled(
                new StringBuilder().Append("connection-string used: ").Append(connectionString).ToString(), LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("script: 20211230_001_Scripta.sql was run", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("script: 20211230_002_Scriptb.sql was run", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);
    }

    [Fact]
    public async Task WhenCreatingNewDatabaseFailsDuringTheMigrationProcess()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(false, new Exception()));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        Result<RunMigrationResult> resultRunMigrations = new(true);
        _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , CancellationToken.None);

        Assert.False(result);

        _ = loggerMock
            .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup database executed with errors", LogLevel.Error, Times.Exactly(1), true)
            .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Never(), false)
            .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), false)
            .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never(), false)
            .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);
    }


    [Fact]
    public async Task WhenOneOfTheScriptsCannotBeParsedDuringTheMigrationProcess()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(x => x.TryGetScriptsFromAssembly(It.IsAny<Type>())).Throws(new ArgumentException());

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        Result<RunMigrationResult> resultRunMigrations = new(true);
        _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , CancellationToken.None);

        Assert.False(result);

        _ = loggerMock
            .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Never(), false)
            .CheckIfLoggerWasCalled("One or more scripts could not be loaded, is the sequence patterns correct?", LogLevel.Error, Times.Exactly(1), true)
            .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);
    }

    [Fact]
    public async Task WhenCreatingNewVersioningTableFailsDuringTheMigrationProcess()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(false, new Exception()));

        Result<RunMigrationResult> resultRunMigrations = new(true);
        _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()
                )).ReturnsAsync(resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , CancellationToken.None);

        Assert.False(result);

        _ = loggerMock
           .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
           .CheckIfLoggerWasCalled("setup versioning table executed with errors", LogLevel.Error, Times.Exactly(1), true)
           .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Never(), false)
           .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Never(), false)
           .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);
    }

    [Fact]
    public async Task WhenSomeScriptFailsToRunSkipTheRestOfTheScripts()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        Script script3 = new("20211230_003_Script3.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
          , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.MigrationScriptExecuted))
            .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted, new Exception()));

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType, config, CancellationToken.None);
        Assert.False(result);

        _ = loggerMock
              .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was not completed due to exception", LogLevel.Error, Times.Exactly(1), true)
              .CheckIfLoggerWasCalled("script: 20211230_003_Script3.sql was skipped due to exception in previous script", LogLevel.Warning, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);

        //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
        databaseConnectorMock.Verify(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<Script>()
            , It.IsAny<DateTimeOffset>()
             , It.IsAny<CancellationToken>())
        , Times.Exactly(2));
    }

    [Fact]
    public async Task CanCancelTheMigrationProcessBeforeTheFirstScriptHasRun()
    {
        const string databaseName = "EasyDbMigrator";
        using CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        Script script3 = new("20211230_003_Script3.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
          , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.MigrationScriptExecuted))
            .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.MigrationScriptExecuted));

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        source.Cancel();

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , token);

        Assert.True(result);

        _ = loggerMock
              .CheckIfLoggerWasCalled("migration process was canceled from the outside", LogLevel.Warning, Times.Exactly(1), false);

        //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
        databaseConnectorMock.Verify(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<Script>()
            , It.IsAny<DateTimeOffset>()
             , It.IsAny<CancellationToken>())
        , Times.Exactly(0));
    }

    [Fact]
    public async Task CanCancelTheMigrationProcessAfterTheFirstScriptHasRun()
    {
        const string databaseName = "EasyDbMigrator";
        using CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        Script script3 = new("20211230_003_Script3.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
          , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.MigrationScriptExecuted))
            .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.MigrationWasCancelled, new Exception()));

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , token);

        Assert.False(result);

        _ = loggerMock
              .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("migration process was canceled", LogLevel.Warning, Times.Exactly(1), false)
              .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);

        //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
        databaseConnectorMock.Verify(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<Script>()
            , It.IsAny<DateTimeOffset>()
             , It.IsAny<CancellationToken>())
        , Times.Exactly(2));
    }

    [Fact]
    public async Task CanSkipScriptsIfExecutedBefore()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        Script script3 = new("20211230_003_Script3.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<Script>()
                 , It.IsAny<DateTimeOffset>()
                 , It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.MigrationScriptExecuted))
                 .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun))
                 .ReturnsAsync(new Result<RunMigrationResult>(true, RunMigrationResult.MigrationScriptExecuted));

        DateTimeOffset executedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , CancellationToken.None);

        Assert.True(result);

        _ = loggerMock
             .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
             .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
             .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
             .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1), false)
             .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
             .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), false)
             .CheckIfLoggerWasCalled("script: 20211230_003_Script3.sql was run", LogLevel.Information, Times.Exactly(1), false)
             .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);
    }

    [Fact]
    public async Task CanExcludeScriptsSoTheyWillNotBeExecuted()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new("connection"
            , databaseName);

        Type? someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new("20211230_001_Script1.sql", "some content");
        Script script2 = new("20211230_002_Script2.sql", "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult(scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(new Result<bool>(true));

        _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(true));

        Result<RunMigrationResult> resultRunMigrations = new(true);
        _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                 , It.IsAny<CancellationToken>()
                 )).ReturnsAsync(resultRunMigrations);

        DateTime executedDataTime = new(2021, 12, 31, 2, 16, 0);

        Mock<IDateTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        //exclude some scripts
        migrator.ExcludeTheseScriptsInRun(new List<string> { "20211230_001_Script1.sql" });

        bool result = await migrator.TryApplyMigrationsAsync(someType
            , config
            , CancellationToken.None);
        Assert.True(result);

        _ = loggerMock
            .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Never(), false)
            .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), false)
            .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);
    }

    [Fact]
    public async Task LogWhenSomethingGoesWrongDeletingTheDatabase()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connection";

        MigrationConfiguration config = new(connectionString
            , databaseName);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

        var directoryHelperMock = new Mock<IDirectoryHelper>();
        Mock<IDateTimeHelper> datetimeHelperMock = new();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
           , It.IsAny<CancellationToken>()
           )).ReturnsAsync(new Result<bool>(false, new Exception()));

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool success = await migrator.TryDeleteDatabaseIfExistAsync(config
            , CancellationToken.None);

        Assert.False(success);

        _ = loggerMock
            .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync executed with error", LogLevel.Error, Times.Exactly(1), true);
    }

    [Fact]
    public async Task CanDeleteOldTestDatabaseToSetupCleanTest()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connection";

        MigrationConfiguration config = new(connectionString
            , databaseName);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        Mock<IDateTimeHelper> datetimeHelperMock = new();

        _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
             )).ReturnsAsync(new Result<bool>(true));

        DbMigrator migrator = new(loggerMock.Object
            , databaseConnectorMock.Object
            , assemblyResourceHelperMock.Object
            , directoryHelperMock.Object
            , datetimeHelperMock.Object);

        bool success = await migrator.TryDeleteDatabaseIfExistAsync(config
            , CancellationToken.None);

        Assert.True(success);

        _ = loggerMock
            .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), false);
    }

    [Fact]
    public void IsPossibleToMockEasyDbCreatorInYourOwnTestsUsingTheInterface()
    {
        var sut = new Mock<IDbMigrator>();

        _ = sut.Setup(x => x.ExcludeTheseScriptsInRun(It.IsAny<List<string>>()))
         .Throws(new Exception());

        _ = sut.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
          .ThrowsAsync(new Exception());

        _ = sut.Setup(x => x.TryApplyMigrationsAsync(It.IsAny<Type>()
            , It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
           .ThrowsAsync(new Exception());

        //just proving it can be done, no extra assert is needed
    }

    [Fact]
    public void IsPossibleToMockEasyDbCreatorInYourOwnTestsUsingConcreteClass()
    {
        var sut = new Mock<DbMigrator>();

        _ = sut.Setup(x => x.ExcludeTheseScriptsInRun(It.IsAny<List<string>>()))
       .Throws(new Exception());

        _ = sut.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
          .ThrowsAsync(new Exception());

        _ = sut.Setup(x => x.TryApplyMigrationsAsync(It.IsAny<Type>()
            , It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
           .ThrowsAsync(new Exception());

        //just proving it can be done, no extra assert is needed
    }

    private sealed class TestLoggerImplementation : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new Exception();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new Exception();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new Exception();
        }
    }

    [Fact]
    public void IsPossibleToInjectEasyDbCreatorInServiceCollection()
    {
        ServiceCollection? collection = new();
        _ = collection.AddTransient<ILogger, TestLoggerImplementation>();
        _ = collection.AddTransient<IDatabaseConnector, MicrosoftSqlConnector>();
        _ = collection.AddTransient<IAssemblyResourceHelper, AssemblyResourceHelper>();
        _ = collection.AddTransient<IDirectoryHelper, DirectoryHelper>();
        _ = collection.AddTransient<IDateTimeHelper, DateTimeHelper>();
        _ = collection.AddTransient<IDbMigrator, DbMigrator>();

        using ServiceProvider? serviceProvider = collection.BuildServiceProvider();

        IDbMigrator? migrator = serviceProvider.GetService<IDbMigrator>();
        migrator.ExcludeTheseScriptsInRun(new List<string>());
    }
}