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

namespace EasyDbMigratorTests.Unittests
{
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

                DbMigrator migrator = new(loggerMock.Object
                    , new MicrosoftSqlConnector()
                    , new AssemblyResourceHelper()
                    , null
                    , datetimeHelperMock.Object);
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

                DbMigrator migrator = new(loggerMock.Object
                    , null
                    , new AssemblyResourceHelper()
                    , directoryHelper
                    , datetimeHelperMock.Object);
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

                DbMigrator migrator = new(loggerMock.Object
                    , new MicrosoftSqlConnector()
                    , null
                    , directoryHelper
                    , datetimeHelperMock.Object);
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

                DbMigrator migrator = new(null
                    , new MicrosoftSqlConnector()
                    , new AssemblyResourceHelper()
                    , directoryHelper
                    , datetimeHelperMock.Object);
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

                DbMigrator migrator = new (loggerMock.Object
                    , new MicrosoftSqlConnector()
                    , new AssemblyResourceHelper()
                    , directoryHelper
                    , null);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_using_the_create_method_happy_flow()
        {
            var act = () =>
            {
                MigrationConfiguration config = new("connection string"
                    , "databaseName");
                var loggerMock = new Mock<ILogger<DbMigrator>>();

                var migrator = DbMigrator.Create(config
                    , loggerMock.Object
                    , new MicrosoftSqlConnector());
            };

            _ = act.Should().NotThrow<ArgumentNullException>();
            _ = act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void When_using_the_create_method_the_ILogger_Should_be_provided()
        {
            var act = () =>
            {
                MigrationConfiguration config = new("connection string"
                    , "databaseName");
                DbMigrator.Create(config
                    , null
                    , new PostgreSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_using_the_create_method_the_MigrationConfiguration_should_be_provided()
        {
            var act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                var migrator = DbMigrator.Create(null
                    , loggerMock.Object
                    , new MicrosoftSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_using_the_create_method_the_IDatabaseConnector_should_be_provided()
        {
            var act = () =>
            {
                MigrationConfiguration config = new("connection string"
                  , "databaseName");
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                var migrator = DbMigrator.Create(config
                    , loggerMock.Object
                    , null);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_using_the_CreateForLocalIntegrationTesting_method_happy_flow()
        {
            var act = () =>
            {
                MigrationConfiguration config = new("connection string"
                    , "databaseName");
                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> dataTimeHelperMock = new();
                DbMigrator.CreateForLocalIntegrationTesting(config
                    , loggerMock.Object
                    , dataTimeHelperMock.Object
                    , new MicrosoftSqlConnector());
            };

            _ = act.Should().NotThrow<ArgumentNullException>();
            _ = act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void When_using_the_CreateForLocalIntegrationTesting_method_the_ILogger_should_be_provided()
        {
            var act = () =>
            {
                MigrationConfiguration config = new("connection string"
                    , "databaseName");
                Mock<IDataTimeHelper> dataTimeHelperMock = new();

                var migrator = DbMigrator.CreateForLocalIntegrationTesting(config
                    , null
                    , dataTimeHelperMock.Object
                    , new MicrosoftSqlConnector());
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
                DbMigrator.CreateForLocalIntegrationTesting(null
                    , loggerMock.Object
                    , dataTimeHelperMock.Object
                    , new PostgreSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_using_the_CreateForLocalIntegrationTesting_method_the_dataTimeHelper_Should_be_provided()
        {
            var act = () =>
            {
                MigrationConfiguration config = new("connection string"
                    , "databaseName");

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator.CreateForLocalIntegrationTesting(config
                    , loggerMock.Object
                    , null
                    , new MicrosoftSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_using_the_CreateForLocalIntegrationTesting_method_the_IDateBaseConnector_should_be_provided()
        {
            var act = () =>
            {
                MigrationConfiguration config = new("connection string"
                    , "databasename");
                Mock<IDataTimeHelper> dataTimeHelperMock = new();

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator.CreateForLocalIntegrationTesting(config
                    , loggerMock.Object
                    , dataTimeHelperMock.Object
                    , null);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task When_migration_process_goes_ok()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionString = "connectionString";

            MigrationConfiguration config = new(connectionString
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled($"connection-string used: {connectionString}", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);
        }

        [Fact]
        public async Task Can_use_fileDirectory_for_scripts()
        {
            const string DATABASE_NAME = "EasyDbMigrator";
            const string connectionString = "connectionString";

            MigrationConfiguration config = new(connectionString
                , DATABASE_NAME
                , "some directory");

            var someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new(@"20211230_001_Scripta.sql", "some content");
            Script script2 = new(@"20211230_002_Scriptb.sql", "some content");
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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled($"connection-string used: {connectionString}", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled(@"script: 20211230_001_Scripta.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled(@"script: 20211230_002_Scriptb.sql was run", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), false);
        }

        [Fact]
        public async Task When_creating_new_database_fails_during_the_migration_process()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeFalse();

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup database executed with errors", LogLevel.Error, Times.Exactly(1), true)
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Never(), false)
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), false)
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never(), false)
                .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);
        }


        [Fact]
        public async Task When_one_of_the_scripts_cannot_be_parsed_during_the_migration_process()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeFalse();

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Never(), false)
                .CheckIfLoggerWasCalled("One or more scripts could not be loaded, is the sequence patterns correct?", LogLevel.Error, Times.Exactly(1), true)
                .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);
        }

        [Fact]
        public async Task When_creating_new_versioningTable_fails_during_the_migration_process()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeFalse();

            _ = loggerMock
               .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), false)
               .CheckIfLoggerWasCalled("setup versioning table executed with errors", LogLevel.Error, Times.Exactly(1), true)
               .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Never(), false)
               .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Never(), false)
               .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), false);
        }

        [Fact]
        public async Task When_some_script_fails_to_run_skip_the_rest_of_the_scripts()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType, config, CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeFalse();

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
        public async Task Can_cancel_the_migration_process_before_the_first_script_has_run()
        {
            const string databaseName = "EasyDbMigrator";
            using CancellationTokenSource source = new();
            var token = source.Token;

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            source.Cancel();

            bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , token).ConfigureAwait(true);

            _ = result.Should().BeTrue();

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
        public async Task Can_cancel_the_migration_process_after_the_first_script_has_run()
        {
            const string databaseName = "EasyDbMigrator";
            using CancellationTokenSource source = new();
            var token = source.Token;

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType
                    , config
                    , token).ConfigureAwait(true);

            _ = result.Should().BeFalse();

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
        public async Task Can_skip_scripts_if_executed_before()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            Mock<IDataTimeHelper> datetimeHelperMock = new();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(executedDataTime);

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool result = await migrator.TryApplyMigrationsAsync(someType
                , config
                , CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeTrue();

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
        public async Task Can_exclude_scripts_so_they_will_not_be_executed()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new("connection"
                , databaseName);

            var someType = typeof(DbMigratorTests);

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

            Mock<IDataTimeHelper> datetimeHelperMock = new();
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
                , CancellationToken.None).ConfigureAwait(true);

            _ = result.Should().BeTrue();

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
        public async Task Log_when_something_goes_wrong_deleting_the_database()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionString = "connection";

            MigrationConfiguration config = new(connectionString
                , databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

            var directoryHelperMock = new Mock<IDirectoryHelper>();
            Mock<IDataTimeHelper> datetimeHelperMock = new();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
               , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(false, new Exception()));

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool success = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None).ConfigureAwait(true);

            _ = success.Should().BeFalse();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync executed with error", LogLevel.Error, Times.Exactly(1), true);
        }

        [Fact]
        public async Task Can_delete_old_testDatabase_to_setup_clean_test()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionString = "connection";

            MigrationConfiguration config = new(connectionString
                , databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            Mock<IDataTimeHelper> datetimeHelperMock = new();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
                 )).ReturnsAsync(new Result<bool>(true));

            DbMigrator migrator = new(loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , directoryHelperMock.Object
                , datetimeHelperMock.Object);

            bool success = await migrator.TryDeleteDatabaseIfExistAsync(config
                , CancellationToken.None).ConfigureAwait(true);

            _ = success.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), false);
        }

        [Fact]
        public void Is_possible_to_mock_EasyDbCreator_in_your_own_tests_using_the_interface()
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
        public void Is_possible_to_mock_EasyDbCreator_in_your_own_tests_using_concrete_class()
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

        private class TestLoggerImplementation : ILogger
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
            migrator.ExcludeTheseScriptsInRun(new List<string>());
        }
    }
}