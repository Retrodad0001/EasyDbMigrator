﻿using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.Integrationtests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace EasyDbMigrator
{
    [ExcludeFromCodeCoverage]
    public class DbMigratorTests
    {
        [Fact]
        public void when_constructing_the_parameter_connector_schould_be_provided()
        {
            Action act = () =>
            {

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration(connectionString: "connection-string"
                    , databaseName: "databasename")
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , databaseconnector: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , assemblyResourceHelper: new AssemblyResourceHelper()
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
                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration(connectionString: "connection-string"
                        , databaseName: "databasename")
                    , databaseconnector: new SqlDbConnector()
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , assemblyResourceHelper: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = new DbMigrator(logger: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , migrationConfiguration: new MigrationConfiguration(connectionString: "connection-string"
                        , databaseName: "databasename")
                    , databaseconnector: new SqlDbConnector()
                    , assemblyResourceHelper: new AssemblyResourceHelper()
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
                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration(connectionString: "connection-string"
                        , databaseName: "databasename")
                    , databaseconnector: new SqlDbConnector()
                    , assemblyResourceHelper: new AssemblyResourceHelper()
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , dataTimeHelper: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_constructing_migrations_the_migrationConfiguration_should_be_provided()
        {
            Action act = () =>
            {
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , migrationConfiguration: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , databaseconnector: new SqlDbConnector()
                    , assemblyResourceHelper: new AssemblyResourceHelper()
                    , dataTimeHelper: datetimeHelperMock.Object);
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
                    , databaseConnector: new SqlDbConnector());
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

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config
                    , logger: null
                    , databaseConnector: new PostgreSqlConnector());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_create_method_the_migrationconfiguration_schould_be_provided()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: null
                    , logger: loggerMock.Object
                    , databaseConnector: new SqlDbConnector());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , databaseConnector: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
                    , databaseConnector: new SqlDbConnector());
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , logger: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , dataTimeHelperMock: dataTimeHelperMock.Object
                    , databaseConnector: new SqlDbConnector());
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

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , dataTimeHelperMock: null
                    , databaseConnector: new SqlDbConnector());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , dataTimeHelperMock: dataTimeHelperMock.Object
                    , databaseConnector: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task when_everything_goes_ok()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
              )).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
               )).ReturnsAsync(new Result<bool>(isSucces: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task when_creating_new_database_fails()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>(),
              It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
              )).ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception()));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
               )).ReturnsAsync(new Result<bool>(isSucces: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup database when there is none with default settings: error occurred", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true)
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task when_creating_new_versioningtable_fails()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>(),
              It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
              )).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
               )).ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception()));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType);

            _ = loggerMock
               .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
               .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true)
               .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
               .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
               .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task when_some_script_failes_to_run_skip_the_rest_of_the_scripts()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            Script script3 = new Script("20212230_003_Script3.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);
            scripts.Add(script3);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>(),
             It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
              )).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
               )).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, exception: new Exception()));

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType);

            _ = loggerMock
                  .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was not completed due to exception", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true)
                  .CheckIfLoggerWasCalled("script: 20212230_003_Script3.sql was skipped due to exception in previous script", LogLevel.Warning, Times.Exactly(1), checkExceptionNotNull: false)
                  .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: false);

            //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
            databaseConnectorMock.Verify(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTimeOffset>())
            , times: Times.Exactly(2));
        }

        [Fact]
        public async Task when_some_scripts_already_executed_skip_them()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            Script script3 = new Script("20212230_003_Script3.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);
            scripts.Add(script3);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>(),
               It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
              )).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
               )).ReturnsAsync(new Result<bool>(isSucces: true));

           _ = databaseConnectorMock.SetupSequence(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>()))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun, exception: null))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null));

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelper: assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType);

            _ = loggerMock
                 .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("script: 20212230_003_Script3.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                 .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task that_it_possable_to_exclude_Scripts()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupEmptyDataBaseWithDefaultSettingWhenThereIsNoDatabaseAsync(It.IsAny<MigrationConfiguration>()
              )).ReturnsAsync(new Result<bool>(isSucces: true));

            _ = databaseConnectorMock.Setup(x => x.TrySetupDbMigrationsRunTableWhenNotExcistAsync(It.IsAny<MigrationConfiguration>()
               )).ReturnsAsync(new Result<bool>(isSucces: true));

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            //exclude some scripts
            migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: new List<string> { "20212230_001_Script1.sql" });

            _ = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: someType);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

        [Fact]
        public async Task when_using_method_DeleteDatabaseIfExistAsync_Log_the_exception_When_something_goes_wrong()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionstring = "connection";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();
            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>()
              , It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception()));

            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            bool succes = await migrator.TryDeleteDatabaseIfExistAsync(databaseName: databaseName, connectionString: connectionstring);

            _ = succes.Should().BeFalse();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync executed with error", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true);
        }

        [Fact]
        public async Task when_using_method_DeleteDatabaseIfExistAsync_and_nothing_goes_wrong()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionstring = "connection";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();
            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();

            _ = databaseConnectorMock.Setup(x => x.TryDeleteDatabaseIfExistAsync(It.IsAny<string>()
                , It.IsAny<string>())).ReturnsAsync(new Result<bool>(isSucces: true));
                 
            DbMigrator migrator = new DbMigrator(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            bool succes = await migrator.TryDeleteDatabaseIfExistAsync(databaseName: databaseName, connectionString: connectionstring);

            _ = succes.Should().BeTrue();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);
        }

    }
}
