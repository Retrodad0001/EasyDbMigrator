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
    public class DbMigratorTests_V1_1_0
    {
        [Fact]
        public void when_constructing_the_parameter_connector_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {

                ApiVersion apiVersion = ApiVersion.Version1_1_0;
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

                DbMigrator migrator = new(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration(apiVersion: apiVersion
                    , connectionString: "connection-string"
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
        public void when_constructing_the_parameter_assemblyResourceHelper_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                DbMigrator migrator = new(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                        , connectionString: "connection-string"
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
        public void when_constructing_the_parameter_logger_schould_be_provided__V1_1_0()
        {
            Action act = () =>
            {
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = new(logger: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , migrationConfiguration: new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                        , connectionString: "connection-string"
                        , databaseName: "databasename")
                    , databaseconnector: new SqlDbConnector()
                    , assemblyResourceHelper: new AssemblyResourceHelper()
                    , dataTimeHelper: datetimeHelperMock.Object);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_constructing_the_parameter_dataTimeHelper_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = new(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                        , connectionString: "connection-string"
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
        public void when_constructing_migrations_the_migrationConfiguration_should_be_provided__V1_1_0()
        {
            Action act = () =>
            {
                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);
                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = new(logger: loggerMock.Object
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
        public void when_using_the_create_method_happy_flow_V1_1_0()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                    , connectionString: "connection string"
                    , databaseName: "databasename");
                var loggerMock = new Mock<ILogger<DbMigrator>>();

                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config, logger: loggerMock.Object);
            };

            _ = act.Should().NotThrow<ArgumentNullException>();
            _ = act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void when_using_the_create_method_the_ILogger_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                    , connectionString: "connection string"
                    , databaseName: "databasename");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: config, logger: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_create_method_the_migrationconfiguration_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = DbMigrator.Create(migrationConfiguration: null, logger: loggerMock.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_happy_flow_V1_1_0()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                    , connectionString: "connection string"
                    , databaseName: "databasename");
                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> dataTimeHelperMock = new Mock<IDataTimeHelper>();
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: dataTimeHelperMock.Object);
            };

            _ = act.Should().NotThrow<ArgumentNullException>();
            _ = act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_the_ILogger_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                    , connectionString: "connection string"
                    , databaseName: "databasename");
                Mock<IDataTimeHelper> dataTimeHelperMock = new Mock<IDataTimeHelper>();

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , logger: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , dataTimeHelperMock: dataTimeHelperMock.Object);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_the_migrationconfiguration_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                Mock<IDataTimeHelper> dataTimeHelperMock = new Mock<IDataTimeHelper>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: dataTimeHelperMock.Object);
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_using_the_CreateForLocalIntegrationTesting_method_the_dataTimeHelper_schould_be_provided_V1_1_0()
        {
            Action act = () =>
            {
                MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                    , connectionString: "connection string"
                    , databaseName: "databasename");

                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , dataTimeHelperMock: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task when_everything_goes_ok_V1_1_0()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                , connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests_V1_0_0);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            SqlScript script1 = new SqlScript("20212230_001_Script1.sql", "some content");
            SqlScript script2 = new SqlScript("20212230_002_Script2.sql", "some content");
            List<SqlScript> scripts = new List<SqlScript>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<SqlScript>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = databaseConnectorMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<SqlScript>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(logger: loggerMock.Object
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
        public async Task when_creating_new_database_fails_V1_1_0()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                , connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests_V1_0_0);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            SqlScript script1 = new SqlScript("20212230_001_Script1.sql", "some content");
            SqlScript script2 = new SqlScript("20212230_002_Script2.sql", "some content");
            List<SqlScript> scripts = new List<SqlScript>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<SqlScript>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = databaseConnectorMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<SqlScript>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(logger: loggerMock.Object
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
        public async Task when_creating_new_versioningtable_fails_V1_1_0()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                , connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests_V1_0_0);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            SqlScript script1 = new SqlScript("20212230_001_Script1.sql", "some content");
            SqlScript script2 = new SqlScript("20212230_002_Script2.sql", "some content");
            List<SqlScript> scripts = new List<SqlScript>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<SqlScript>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = databaseConnectorMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception())); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<SqlScript>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(logger: loggerMock.Object
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
        public async Task when_some_script_failes_to_run_skip_the_rest_of_the_scripts_V1_1_0()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                , connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests_V1_0_0);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            SqlScript script1 = new SqlScript("20212230_001_Script1.sql", "some content");
            SqlScript script2 = new SqlScript("20212230_002_Script2.sql", "some content");
            SqlScript script3 = new SqlScript("20212230_003_Script3.sql", "some content");
            List<SqlScript> scripts = new List<SqlScript>();
            scripts.Add(script1);
            scripts.Add(script2);
            scripts.Add(script3);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<SqlScript>>(scripts));

            var sqlDbHelperMock = new Mock<IDatabaseConnector>();
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            _ = sqlDbHelperMock.SetupSequence(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<SqlScript>()
                    , It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, exception: new Exception()));

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , sqlDbHelperMock.Object
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
            sqlDbHelperMock.Verify(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<SqlScript>()
                , It.IsAny<DateTimeOffset>())
            , times: Times.Exactly(2));
        }

        [Fact]
        public async Task when_some_scripts_already_executed_skip_them_V1_1_0()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                , connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests_V1_0_0);

            var loggerMock = new Mock<ILogger<DbMigrator>>();

            SqlScript script1 = new SqlScript("20212230_001_Script1.sql", "some content");
            SqlScript script2 = new SqlScript("20212230_002_Script2.sql", "some content");
            SqlScript script3 = new SqlScript("20212230_003_Script3.sql", "some content");
            List<SqlScript> scripts = new List<SqlScript>();
            scripts.Add(script1);
            scripts.Add(script2);
            scripts.Add(script3);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<SqlScript>>(scripts));

            var sqlDbHelperMock = new Mock<IDatabaseConnector>();
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            _ = sqlDbHelperMock.SetupSequence(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<SqlScript>()
                    , It.IsAny<DateTimeOffset>()))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun, exception: null))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null));

            DateTimeOffset ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: sqlDbHelperMock.Object
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
        public async Task that_it_possable_to_exclude_Scripts_V1_1_0()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                , connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests_V1_0_0);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();

            SqlScript script1 = new SqlScript("20212230_001_Script1.sql", "some content");
            SqlScript script2 = new SqlScript("20212230_002_Script2.sql", "some content");
            List<SqlScript> scripts = new List<SqlScript>();
            scripts.Add(script1);
            scripts.Add(script2);

            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = assemblyResourceHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<SqlScript>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = databaseConnectorMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<SqlScript>()
                    , It.IsAny<DateTimeOffset>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
            _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

            DbMigrator migrator = new(logger: loggerMock.Object
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
        public async Task when_using_method_DeleteDatabaseIfExistAsync_Log_the_exception_When_something_goes_wrong_V1_1_0()
        {
            const string databaseName = "EasyDbMigrator";
            const string connectionstring = "connection";

            MigrationConfiguration config = new MigrationConfiguration(apiVersion: ApiVersion.Version1_1_0
                , connectionString: connectionstring
                , databaseName: databaseName);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var databaseConnectorMock = new Mock<IDatabaseConnector>();
            var assemblyResourceHelperMock = new Mock<IAssemblyResourceHelper>();
            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = databaseConnectorMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception()));

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseconnector: databaseConnectorMock.Object
                , assemblyResourceHelperMock.Object
                , dataTimeHelper: datetimeHelperMock.Object);

            bool succes = await migrator.TryDeleteDatabaseIfExistAsync(databaseName: databaseName, connectionString: connectionstring);

            _ = succes.Should().BeFalse();

            _ = loggerMock
                .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync executed with error", LogLevel.Error, Times.Exactly(1), checkExceptionNotNull: true);
        }

    }
}
