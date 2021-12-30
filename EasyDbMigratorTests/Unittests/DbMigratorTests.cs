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
        public void when_constructing_the_parameter_directoryhelper_schould_be_provided()
        {
            Action act = () =>
            {

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
               
                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                    , databaseconnector: new MicrosoftSqlConnector()
                    , assemblyResourceHelper: new AssemblyResourceHelper()
                    , directoryHelper: null
                    , dataTimeHelper: datetimeHelperMock.Object);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_constructing_the_parameter_connector_schould_be_provided()
        {
            Action act = () =>
            {

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                IDirectoryHelper directoryHelper = new DirectoryHelper();

                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                    , databaseconnector: null
                    , assemblyResourceHelper: new AssemblyResourceHelper()
                    , directoryHelper: directoryHelper
                    , dataTimeHelper: datetimeHelperMock.Object);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_constructing_the_parameter_assemblyResourceHelper_schould_be_provided()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                IDirectoryHelper directoryHelper = new DirectoryHelper();

                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                    , databaseconnector: new MicrosoftSqlConnector()
                    , assemblyResourceHelper: null
                    , directoryHelper: directoryHelper
                    , dataTimeHelper: datetimeHelperMock.Object);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_constructing_the_parameter_logger_schould_be_provided()
        {
            Action act = () =>
            {
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                IDirectoryHelper directoryHelper = new DirectoryHelper();

                DbMigrator migrator = new DbMigrator(logger: null
                    , databaseconnector: new MicrosoftSqlConnector()
                    , assemblyResourceHelper: new AssemblyResourceHelper()
                    , directoryHelper: directoryHelper
                    , dataTimeHelper: datetimeHelperMock.Object);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_constructing_the_parameter_dataTimeHelper_schould_be_provided()
        {
            Action act = () =>
            {
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                IDirectoryHelper directoryHelper = new DirectoryHelper();

                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                    , databaseconnector: new MicrosoftSqlConnector()
                    , assemblyResourceHelper: new AssemblyResourceHelper()
                    , directoryHelper: directoryHelper
                    , dataTimeHelper: null);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_create_method_happy_flow()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection string"
                    , databaseName: "databasename");
                var loggerMock = new Mock<ILogger<DbMigrator>>();

                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , databaseConnector: new MicrosoftSqlConnector());
            };

            _ = act.Should().NotThrow<ArgumentNullException>();
            _ = act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void when_using_the_create_method_the_ILogger_schould_be_provided()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection string"
                    , databaseName: "databasename");
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config
                    , logger: null
                    , databaseConnector: new PostgreSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_create_method_the_migrationconfiguration_schould_be_provided()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: null
                    , logger: loggerMock.Object
                    , databaseConnector: new MicrosoftSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_create_method_the_IDatabaseConnector_schould_be_provided()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection string"
                  , databaseName: "databasename");
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , databaseConnector: null);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_happy_flow()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection string"
                    , databaseName: "databasename");
                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> dataTimeHelperMock = new Mock<IDataTimeHelper>();
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: dataTimeHelperMock.Object
                    , databaseConnector: new MicrosoftSqlConnector());
            };

            _ = act.Should().NotThrow<ArgumentNullException>();
            _ = act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_the_ILogger_schould_be_provided()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection string"
                    , databaseName: "databasename");
                Mock<IDataTimeHelper> dataTimeHelperMock = new Mock<IDataTimeHelper>();

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: null
                    , dataTimeHelperMock: dataTimeHelperMock.Object
                    , databaseConnector: new MicrosoftSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_the_migrationconfiguration_schould_be_provided()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> dataTimeHelperMock = new Mock<IDataTimeHelper>();
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: null
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: dataTimeHelperMock.Object
                    , databaseConnector: new PostgreSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_the_dataTimeHelper_schould_be_provided()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection string"
                    , databaseName: "databasename");

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: null
                    , databaseConnector: new MicrosoftSqlConnector());
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_the_IDatebaseConnector_schould_be_provided()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection string"
                    , databaseName: "databasename");
                Mock<IDataTimeHelper> dataTimeHelperMock = new Mock<IDataTimeHelper>();

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: dataTimeHelperMock.Object
                    , databaseConnector: null);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task when_migration_process_goes_ok()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionString = "someconnectionstring";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20211230_001_Script1.sql", "some content");
            Script script2 = new Script("20211230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(wasSuccessful: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                    , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled($"connection-string used: {connectionString}", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task can_use_filedirectory_for_scripts()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionString = "someconnectionstring";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                , databaseName: databaseName
                , ScriptsDirectory: "some directory");

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20211230_001_Scripta.sql", "some content");
            Script script2 = new Script("20211230_002_Scriptb.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            
            var directoryHelperMock = new Mock<IDirectoryHelper>();
            _ = directoryHelperMock.Setup(m => m.TryGetScriptsFromDirectoryAsync(It.IsAny<string>())).ReturnsAsync(() => scripts);
            
            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(wasSuccessful: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                    , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled($"connection-string used: {connectionString}", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20211230_001_Scripta.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Scriptb.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task when_creating_new_database_fails_during_the_migration_proces()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20211230_001_Script1.sql", "some content");
            Script script2 = new Script("20211230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: false, exception: new Exception()));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(wasSuccessful: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                    , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup database executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true)
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: false);
        }


        [Fact]
        public async Task when_one_of_the_scripts_cannot_be_parsed_during_the_migration_proces()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(x => x.TryGetScriptsFromAssembly(It.IsAny<Type>())).Throws(new ArgumentException());

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(wasSuccessful: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                    , It.IsAny<CancellationToken>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);                

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("One or more scripts could not be loaded, is the sequence patterns correct?", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true)
                .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task when_creating_new_versioningtable_fails_during_the_migration_proces()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20211230_001_Script1.sql", "some content");
            Script script2 = new Script("20211230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: false, exception: new Exception()));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(wasSuccessful: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                    , It.IsAny<CancellationToken>()
                    )).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
               .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
               .CheckIfLoggerWasCalled("setup versioning table executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true)
               .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
               .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
               .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task when_some_script_failes_to_run_skip_the_rest_of_the_scripts()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Script script1 = new Script("20211230_001_Script1.sql", "some content");
            Script script2 = new Script("20211230_002_Script2.sql", "some content");
            Script script3 = new Script("20211230_003_Script3.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);
            scripts.Add(script3);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
              , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                    , It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<RunMigrationResult>(wasSuccessful: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                .ReturnsAsync(new Result<RunMigrationResult>(wasSuccessful: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, exception: new Exception()));

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
                  .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was not completed due to exception", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true)
                  .CheckIfLoggerWasCalled("script: 20211230_003_Script3.sql was skipped due to exception in previous script", LogLevel.Warning, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("migration process executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: false);

            //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
            databaseConnectorMock.Verify(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                 , It.IsAny<CancellationToken>())
            , times: Times.Exactly(2));
        }

        [Fact]
        public async Task can_cancel_the_migration_proces_before_the_first_script_has_run()
        {
            const string databaseName = "EasyDbMigrator";
            using CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Script script1 = new Script("20211230_001_Script1.sql", "some content");
            Script script2 = new Script("20211230_002_Script2.sql", "some content");
            Script script3 = new Script("20211230_003_Script3.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);
            scripts.Add(script3);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
              , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                    , It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<RunMigrationResult>(wasSuccessful: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                .ReturnsAsync(new Result<RunMigrationResult>(wasSuccessful: true, RunMigrationResult.MigrationScriptExecuted, exception: null));

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            source.Cancel();

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: token).ConfigureAwait(true);

            _ = loggerMock
                  .CheckIfLoggerWasCalled("migration process was canceled from the outside", LogLevel.Warning, Times.Exactly(1), checkExceptionNotNull: false);

            //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
            databaseConnectorMock.Verify(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>()
                 , It.IsAny<CancellationToken>())
            , times: Times.Exactly(0));
        }

        [Fact]
        public async Task can_skip_scripts_if_executed_before()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Script script1 = new Script("20211230_001_Script1.sql", "some content");
            Script script2 = new Script("20211230_002_Script2.sql", "some content");
            Script script3 = new Script("20211230_003_Script3.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);
            scripts.Add(script3);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                     , It.IsAny<Script>()
                     , It.IsAny<DateTimeOffset>()
                     , It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new Result<RunMigrationResult>(wasSuccessful: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                     .ReturnsAsync(new Result<RunMigrationResult>(wasSuccessful: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun, exception: null))
                     .ReturnsAsync(new Result<RunMigrationResult>(wasSuccessful: true, RunMigrationResult.MigrationScriptExecuted, exception: null));

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
                 .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("script: 20211230_003_Script3.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task can_exclude_scripts_so_they_will_not_be_executed()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20211230_001_Script1.sql", "some content");
            Script script2 = new Script("20211230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryGetScriptsFromAssembly(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>())).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<CancellationToken>()
              )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
                 , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(wasSuccessful: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()
                     , It.IsAny<CancellationToken>()
                     )).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            //exclude some scripts
            migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: new List<string> { "20211230_001_Script1.sql" });

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType
                , migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20211230_001_Script1.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task log_when_something_goes_wrong_deleting_the_database()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionstring = "connection";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            var databaseConnectorMock = new Mock<IDatabaseConnector>();
            
            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            
            var directoryHelperMock = new Mock<IDirectoryHelper>();
            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
               , It.IsAny<CancellationToken>()
               )).ReturnsAsync(new Result<bool>(wasSuccessful: false, exception: new Exception()));

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            bool succes = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = succes.Should().BeFalse();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync executed with error", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true);
        }

        [Fact]
        public async Task can_delete_old_testdatabase_to_setup_clean_test()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionstring = "connection";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
         
            var databaseConnectorMock = new Mock<IDatabaseConnector>();
            
            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();

            var directoryHelperMock = new Mock<IDirectoryHelper>();

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<CancellationToken>()
                 )).ReturnsAsync(new Result<bool>(wasSuccessful: true));

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , directoryHelper: directoryHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            bool succes = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                , cancellationToken: CancellationToken.None).ConfigureAwait(true);

            _ = succes.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public void is_possable_to_mock_EasyDbCreator_in_your_own_tests_using_the_interface()
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
        public void is_possable_to_mock_EasyDbCreator_in_your_own_tests_using_concrete_class()
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

        private class TestloggerImplementation : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                throw new NotImplementedException();
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void is_possable_to_inject_EasyDbCreator_in_ServiceCollection()
        {
            var collection = new ServiceCollection();
            _ = collection.AddTransient<ILogger, TestloggerImplementation>();
            _ = collection.AddTransient<IDatabaseConnector, MicrosoftSqlConnector>();
            _ = collection.AddTransient<IAssemblyResourceHelper, AssemblyResourceHelper>();
            _ = collection.AddTransient<IDirectoryHelper, DirectoryHelper>();
            _ = collection.AddTransient<IDataTimeHelper, DataTimeHelper>();
            _ = collection.AddTransient<IDbMigrator, DbMigrator>();

            using ServiceProvider serviceProvider = collection.BuildServiceProvider();

            var migrator = serviceProvider.GetService<IDbMigrator>();
            migrator.ExcludeTheseScriptsInRun(new List<string>());
        }
    }
}