// Ignore Spelling: versioning Migrator

using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EasyDbMigratorTests.Unittests;

[ExcludeFromCodeCoverage]
public class DbMigratorTests
{
    [Fact]
    public void When_constructing_the_parameter_DirectoryHelper_Should_be_provided()
    {
        var act = () =>
        {

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDataTimeHelper> datetimeHelperMock = new();

            DbMigrator unused = new(logger: loggerMock.Object
                , databaseConnector: new MicrosoftSqlConnector()
                , assemblyResourceHelper: new AssemblyResourceHelper()
                , directoryHelper: null!
                , dataTimeHelper: datetimeHelperMock.Object);
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_constructing_the_parameter_connector_Should_be_provided()
    {
        var act = () =>
        {

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDataTimeHelper> datetimeHelperMock = new();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new(logger: loggerMock.Object
                , databaseConnector: null!
                , assemblyResourceHelper: new AssemblyResourceHelper()
                , directoryHelper: directoryHelper
                , dataTimeHelper: datetimeHelperMock.Object);
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_constructing_the_parameter_assemblyResourceHelper_should_be_provided()
    {
        var act = () =>
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDataTimeHelper> datetimeHelperMock = new();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new(logger: loggerMock.Object
                , databaseConnector: new MicrosoftSqlConnector()
                , assemblyResourceHelper: null!
                , directoryHelper: directoryHelper
                , dataTimeHelper: datetimeHelperMock.Object);
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_constructing_the_parameter_logger_should_be_provided()
    {
        var act = () =>
        {
            Mock<IDataTimeHelper> datetimeHelperMock = new();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new(logger: null!
                , databaseConnector: new MicrosoftSqlConnector()
                , assemblyResourceHelper: new AssemblyResourceHelper()
                , directoryHelper: directoryHelper
                , dataTimeHelper: datetimeHelperMock.Object);
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_constructing_the_parameter_dataTimeHelper_should_be_provided()
    {
        var act = () =>
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            IDirectoryHelper directoryHelper = new DirectoryHelper();

            DbMigrator unused = new (logger: loggerMock.Object
                , databaseConnector: new MicrosoftSqlConnector()
                , assemblyResourceHelper: new AssemblyResourceHelper()
                , directoryHelper: directoryHelper
                , dataTimeHelper: null!);
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_using_the_create_method_happy_flow()
    {
        var act = () =>
        {
            MigrationConfiguration config = new(connectionString: "connection string"
                , databaseName: "databaseName");
            var loggerMock = new Mock<ILogger<DbMigrator>>();

            var unused = DbMigrator.Create(migrationConfiguration: config
                , logger: loggerMock.Object
                , databaseConnector: new MicrosoftSqlConnector());
        };

        _ = act.Should().NotThrow<ArgumentNullException>();
        _ = act.Should().NotThrow<Exception>();
    }

    [Fact]
    public void When_using_the_create_method_the_ILogger_Should_be_provided()
    {
        var act = () =>
        {
            MigrationConfiguration config = new(connectionString: "connection string"
                , databaseName: "databaseName");
            DbMigrator.Create(migrationConfiguration: config
                , logger: null!
                , databaseConnector: new PostgreSqlConnector());
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_using_the_create_method_the_MigrationConfiguration_should_be_provided()
    {
        var act = () =>
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var unused = DbMigrator.Create(migrationConfiguration: null!
                , logger: loggerMock.Object
                , databaseConnector: new MicrosoftSqlConnector());
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_using_the_create_method_the_IDatabaseConnector_should_be_provided()
    {
        var act = () =>
        {
            MigrationConfiguration config = new(connectionString: "connection string"
              , databaseName: "databaseName");
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var unused = DbMigrator.Create(migrationConfiguration: config
                , logger: loggerMock.Object
                , databaseConnector: null!);
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_using_the_CreateForLocalIntegrationTesting_method_happy_flow()
    {
        var act = () =>
        {
            MigrationConfiguration config = new(connectionString: "connection string"
                , databaseName: "databaseName");
            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Mock<IDataTimeHelper> dataTimeHelperMock = new();
            DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: dataTimeHelperMock.Object
                , databaseConnector: new MicrosoftSqlConnector());
        };

        _ = act.Should().NotThrow<ArgumentNullException>();
        _ = act.Should().NotThrow<Exception>();
    }

    [Fact]
    public void When_using_the_CreateForLocalIntegrationTesting_method_the_ILogger_should_be_provided()
    {
        var act = () =>
        {
            MigrationConfiguration config = new(connectionString: "connection string"
                , databaseName: "databaseName");
            Mock<IDataTimeHelper> dataTimeHelperMock = new();

            var unused = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: null!
                , dataTimeHelperMock: dataTimeHelperMock.Object
                , databaseConnector: new MicrosoftSqlConnector());
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_using_the_CreateForLocalIntegrationTesting_method_the_MigrationConfiguration_Should_be_provided()
    {
        var act = () =>
        {
            var loggerMock = new Mock<ILogger<DbMigrator>>();
            Mock<IDataTimeHelper> dataTimeHelperMock = new();
            DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: null!
                , logger: loggerMock.Object
                , dataTimeHelperMock: dataTimeHelperMock.Object
                , databaseConnector: new PostgreSqlConnector());
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_using_the_CreateForLocalIntegrationTesting_method_the_dataTimeHelper_Should_be_provided()
    {
        var act = () =>
        {
            MigrationConfiguration config = new(connectionString: "connection string"
                , databaseName: "databaseName");

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: null!
                , databaseConnector: new MicrosoftSqlConnector());
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void When_using_the_CreateForLocalIntegrationTesting_method_the_IDateBaseConnector_should_be_provided()
    {
        var act = () =>
        {
            MigrationConfiguration config = new(connectionString: "connection string"
                , databaseName: "databasename");
            Mock<IDataTimeHelper> dataTimeHelperMock = new();

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: loggerMock.Object
                , dataTimeHelperMock: dataTimeHelperMock.Object
                , databaseConnector: null!);
        };

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task When_migration_process_goes_ok()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connectionString";

        MigrationConfiguration config = new(connectionString: connectionString
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        Result<RunMigrationResult> resultRunMigrations = new(wasSuccessful: true);
        _ = databaseConnectorMock.Setup(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(value: resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeTrue();

        _ = loggerMock
            .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: $"connection-string used: {connectionString}", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "Total scripts found: 2", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }

    [Fact]
    public async Task Can_use_fileDirectory_for_scripts()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connectionString";

        MigrationConfiguration config = new(connectionString: connectionString
            , databaseName: databaseName
            , scriptsDirectory: "some directory");

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        // ReSharper disable once StringLiteralTypo
        Script script1 = new(filename: @"20211230_001_Scripta.sql", content: "some content");
        // ReSharper disable once StringLiteralTypo
        Script script2 = new(filename: @"20211230_002_Scriptb.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

        var directoryHelperMock = new Mock<IDirectoryHelper>();
        _ = directoryHelperMock.Setup(expression: m => m.TryGetScriptsFromDirectoryAsync(It.IsAny<string>())).ReturnsAsync(valueFunction: () => scripts);

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        Result<RunMigrationResult> resultRunMigrations = new(wasSuccessful: true);
        _ = databaseConnectorMock.Setup(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(value: resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeTrue();

        _ = loggerMock
            .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: $"connection-string used: {connectionString}", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "Total scripts found: 2", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            // ReSharper disable once StringLiteralTypo
            .CheckIfLoggerWasCalled(expectedMessage: @"script: 20211230_001_Scripta.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            // ReSharper disable once StringLiteralTypo
            .CheckIfLoggerWasCalled(expectedMessage: @"script: 20211230_002_Scriptb.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }

    [Fact]
    public async Task When_creating_new_database_fails_during_the_migration_process()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: false, exception: new Exception()));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        Result<RunMigrationResult> resultRunMigrations = new(wasSuccessful: true);
        _ = databaseConnectorMock.Setup(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(value: resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeFalse();

        _ = loggerMock
            .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup database executed with errors", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: true)
            .CheckIfLoggerWasCalled(expectedMessage: "setup DbMigrationsRun when there is none executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Never(), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "script: 20212230_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Never(), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "script: 20212230_002_Script2.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Never(), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "migration process executed with errors", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }


    [Fact]
    public async Task When_one_of_the_scripts_cannot_be_parsed_during_the_migration_process()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: x => x.TryGetScriptsFromAssembly(It.IsAny<Type>())).Throws(exception: new ArgumentException());

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        Result<RunMigrationResult> resultRunMigrations = new(wasSuccessful: true);
        _ = databaseConnectorMock.Setup(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(value: resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeFalse();

        _ = loggerMock
            .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "Total scripts found: 2", expectedLogLevel: LogLevel.Information, times: Times.Never(), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "One or more scripts could not be loaded, is the sequence patterns correct?", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: true)
            .CheckIfLoggerWasCalled(expectedMessage: "migration process executed with errors", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }

    [Fact]
    public async Task When_creating_new_versioningTable_fails_during_the_migration_process()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: false, exception: new Exception()));

        Result<RunMigrationResult> resultRunMigrations = new(wasSuccessful: true);
        _ = databaseConnectorMock.Setup(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()
                )).ReturnsAsync(value: resultRunMigrations);

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeFalse();

        _ = loggerMock
           .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
           .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed with errors", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: true)
           .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Never(), checkExceptionNotNull: false)
           .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Never(), checkExceptionNotNull: false)
           .CheckIfLoggerWasCalled(expectedMessage: "migration process executed with errors", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }

    [Fact]
    public async Task When_some_script_fails_to_run_skip_the_rest_of_the_scripts()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        Script script3 = new(filename: "20211230_003_Script3.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
          , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.SetupSequence(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationScriptExecuted))
            .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.ExceptionWasThrownWhenScriptWasExecuted, exception: new Exception()));

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType, migrationConfiguration: config, cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeFalse();

        _ = loggerMock
              .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "Total scripts found: 3", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2.sql was not completed due to exception", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: true)
              .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_003_Script3.sql was skipped due to exception in previous script", expectedLogLevel: LogLevel.Warning, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "migration process executed with errors", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);

        //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
        databaseConnectorMock.Verify(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<Script>()
            , It.IsAny<DateTimeOffset>()
             , It.IsAny<CancellationToken>())
        , times: Times.Exactly(callCount: 2));
    }

    [Fact]
    public async Task Can_cancel_the_migration_process_before_the_first_script_has_run()
    {
        const string databaseName = "EasyDbMigrator";
        using CancellationTokenSource source = new();
        var token = source.Token;

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        Script script3 = new(filename: "20211230_003_Script3.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
          , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.SetupSequence(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationScriptExecuted))
            .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationScriptExecuted));

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        source.Cancel();

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: token).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeTrue();

        _ = loggerMock
              .CheckIfLoggerWasCalled(expectedMessage: "migration process was canceled from the outside", expectedLogLevel: LogLevel.Warning, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);

        //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
        databaseConnectorMock.Verify(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<Script>()
            , It.IsAny<DateTimeOffset>()
             , It.IsAny<CancellationToken>())
        , times: Times.Exactly(callCount: 0));
    }

    [Fact]
    public async Task Can_cancel_the_migration_process_after_the_first_script_has_run()
    {
        const string databaseName = "EasyDbMigrator";
        using CancellationTokenSource source = new();
        var token = source.Token;

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        Script script3 = new(filename: "20211230_003_Script3.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
          , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.SetupSequence(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                , It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationScriptExecuted))
            .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationWasCancelled, exception: new Exception()));

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: token).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeFalse();

        _ = loggerMock
              .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "Total scripts found: 3", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "migration process was canceled", expectedLogLevel: LogLevel.Warning, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
              .CheckIfLoggerWasCalled(expectedMessage: "migration process executed with errors", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);

        //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
        databaseConnectorMock.Verify(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<Script>()
            , It.IsAny<DateTimeOffset>()
             , It.IsAny<CancellationToken>())
        , times: Times.Exactly(callCount: 2));
    }

    [Fact]
    public async Task Can_skip_scripts_if_executed_before()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        Script script3 = new(filename: "20211230_003_Script3.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2,
            script3
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.SetupSequence(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<Script>()
                 , It.IsAny<DateTimeOffset>()
                 , It.IsAny<CancellationToken>()))
                 .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationScriptExecuted))
                 .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.ScriptSkippedBecauseAlreadyRun))
                 .ReturnsAsync(value: new Result<RunMigrationResult>(wasSuccessful: true, value: RunMigrationResult.MigrationScriptExecuted));

        DateTimeOffset executedDataTime = new DateTime(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeTrue();

        _ = loggerMock
             .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
             .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
             .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
             .CheckIfLoggerWasCalled(expectedMessage: "Total scripts found: 3", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
             .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
             .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2.sql was not run because script was already executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
             .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_003_Script3.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
             .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }

    [Fact]
    public async Task Can_exclude_scripts_so_they_will_not_be_executed()
    {
        const string databaseName = "EasyDbMigrator";

        MigrationConfiguration config = new(connectionString: "connection"
            , databaseName: databaseName);

        var someType = typeof(DbMigratorTests);

        var loggerMock = new Mock<ILogger<DbMigrator>>();
        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        Script script1 = new(filename: "20211230_001_Script1.sql", content: "some content");
        Script script2 = new(filename: "20211230_002_Script2.sql", content: "some content");
        List<Script> scripts = new()
        {
            script1,
            script2
        };

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
        _ = assemblyResourceHelperMock.Setup(expression: m => m.TryGetScriptsFromAssembly(someType)).Returns(valueFunction: () => Task.FromResult(result: scripts));

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>())).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
          )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        _ = databaseConnectorMock.Setup(expression: x => x.TrySetupDbMigrationsRunTableWhenNotExistAsync(It.IsAny<MigrationConfiguration>()
             , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        Result<RunMigrationResult> resultRunMigrations = new(wasSuccessful: true);
        _ = databaseConnectorMock.Setup(expression: x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                 , It.IsAny<CancellationToken>()
                 )).ReturnsAsync(value: resultRunMigrations);

        DateTime executedDataTime = new(year: 2021, month: 12, day: 31, hour: 2, minute: 16, second: 0);

        Mock<IDataTimeHelper> datetimeHelperMock = new();
        _ = datetimeHelperMock.Setup(expression: x => x.GetCurrentUtcTime()).Returns(value: executedDataTime);

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        //exclude some scripts
        migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: new List<string> { "20211230_001_Script1.sql" });

        bool result = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
            , migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = result.Should().BeTrue();

        _ = loggerMock
            .CheckIfLoggerWasCalled(expectedMessage: "start running migrations for database: EasyDbMigrator", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup database executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "setup versioning table executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "Total scripts found: 2", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_001_Script1.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Never(), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "script: 20211230_002_Script2.sql was run", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false)
            .CheckIfLoggerWasCalled(expectedMessage: "migration process executed successfully", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }

    [Fact]
    public async Task Log_when_something_goes_wrong_deleting_the_database()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connection";

        MigrationConfiguration config = new(connectionString: connectionString
            , databaseName: databaseName);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

        var directoryHelperMock = new Mock<IDirectoryHelper>();
        Mock<IDataTimeHelper> datetimeHelperMock = new();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
           , It.IsAny<CancellationToken>()
           )).ReturnsAsync(value: new Result<bool>(wasSuccessful: false, exception: new Exception()));

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool success = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = success.Should().BeFalse();

        _ = loggerMock
            .CheckIfLoggerWasCalled(expectedMessage: "DeleteDatabaseIfExistAsync executed with error", expectedLogLevel: LogLevel.Error, times: Times.Exactly(callCount: 1), checkExceptionNotNull: true);
    }

    [Fact]
    public async Task Can_delete_old_testDatabase_to_setup_clean_test()
    {
        const string databaseName = "EasyDbMigrator";
        const string connectionString = "connection";

        MigrationConfiguration config = new(connectionString: connectionString
            , databaseName: databaseName);

        var loggerMock = new Mock<ILogger<DbMigrator>>();

        var databaseConnectorMock = new Mock<IDatabaseConnector>();

        var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

        var directoryHelperMock = new Mock<IDirectoryHelper>();

        Mock<IDataTimeHelper> datetimeHelperMock = new();

        _ = databaseConnectorMock.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()
             )).ReturnsAsync(value: new Result<bool>(wasSuccessful: true));

        DbMigrator migrator = new(logger: loggerMock.Object
            , databaseConnector: databaseConnectorMock.Object
            , assemblyResourceHelper: assemblyResourceHelperMock.Object
            , directoryHelper: directoryHelperMock.Object
            , dataTimeHelper: datetimeHelperMock.Object);

        bool success = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
            , cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);

        _ = success.Should().BeTrue();

        _ = loggerMock
            .CheckIfLoggerWasCalled(expectedMessage: "DeleteDatabaseIfExistAsync has executed", expectedLogLevel: LogLevel.Information, times: Times.Exactly(callCount: 1), checkExceptionNotNull: false);
    }

    [Fact]
    public void Is_possible_to_mock_EasyDbCreator_in_your_own_tests_using_the_interface()
    {
        var sut = new Mock<IDbMigrator>();

        _ = sut.Setup(expression: x => x.ExcludeTheseScriptsInRun(It.IsAny<List<string>>()))
         .Throws(exception: new Exception());

        _ = sut.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
          .ThrowsAsync(exception: new Exception());

        _ = sut.Setup(expression: x => x.TryApplyMigrationsAsync(It.IsAny<Type>()
            , It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
           .ThrowsAsync(exception: new Exception());

        //just proving it can be done, no extra assert is needed
    }

    [Fact]
    public void Is_possible_to_mock_EasyDbCreator_in_your_own_tests_using_concrete_class()
    {
        var sut = new Mock<DbMigrator>();

        _ = sut.Setup(expression: x => x.ExcludeTheseScriptsInRun(It.IsAny<List<string>>()))
       .Throws(exception: new Exception());

        _ = sut.Setup(expression: x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
          .ThrowsAsync(exception: new Exception());

        _ = sut.Setup(expression: x => x.TryApplyMigrationsAsync(It.IsAny<Type>()
            , It.IsAny<MigrationConfiguration>()
            , It.IsAny<CancellationToken>()))
           .ThrowsAsync(exception: new Exception());

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
    public void Is_possible_to_inject_EasyDbCreator_in_ServiceCollection()
    {
        var collection = new ServiceCollection();
        _ = collection.AddTransient<ILogger, TestLoggerImplementation>();
        _ = collection.AddTransient<IDatabaseConnector, MicrosoftSqlConnector>();
        _ = collection.AddTransient<IAssemblyResourceHelper, AssemblyResourceHelper>();
        _ = collection.AddTransient<IDirectoryHelper, DirectoryHelper>();
        _ = collection.AddTransient<IDataTimeHelper, DataTimeHelper>();
        _ = collection.AddTransient<IDbMigrator, DbMigrator>();

        using var serviceProvider = collection.BuildServiceProvider();

        var migrator = serviceProvider.GetService<IDbMigrator>();
        migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: new List<string>());
    }
}