using EasyDbMigrator;
using EasyDbMigrator.Helpers;
using EasyDbMigrator.Infra;
using EasyDbMigratorTests.Integrationtests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace EasyDbMigratorTests.Unittests
{
    [ExcludeFromCodeCoverage]
    public class DbMigratorTests
    {
        [Fact]
        public void when_creating_the_parameter_sqlDbHelper_cannot_be_null()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = new(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration("connection-string", "databasename")
                    , databaseconnector: null, scriptsHelper: new AssemblyResourceHelper());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_creating_the_parameter_scriptsHelper_cannot_be_null()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = new(logger: loggerMock.Object
                    , migrationConfiguration: new MigrationConfiguration("connection-string", "databasename")
                    , databaseconnector: new SqlDbConnector()
                    , scriptsHelper: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_creating_the_parameter_logger_cannot_be_null()
        {
            Action act = () =>
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                DbMigrator migrator = new(logger: null
                    , migrationConfiguration: new MigrationConfiguration("connection-string", "databasename")
                    , databaseconnector: new SqlDbConnector()
                    , scriptsHelper: new AssemblyResourceHelper());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };

            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void when_running_migrations_the_migrationConfiguration_should_be_provided()
        {
            Action act = () =>
            {
                var loggerMock = new Mock<ILogger<DbMigrator>>();
                DbMigrator migrator = new(logger: loggerMock.Object
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    , migrationConfiguration: null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    , databaseconnector: new SqlDbConnector()
                    , scriptsHelper: new AssemblyResourceHelper());
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

            var scriptsHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = databaseConnectorMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseConnectorMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
                .CheckIfLoggerWasCalled("start running migrations for database: EasyDbMigrator", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("Total scripts found: 2", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1));
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

            var scriptsHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = databaseConnectorMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = databaseConnectorMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , databaseConnectorMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock //TODO when runs enable rest
                .CheckIfLoggerWasCalled("setup database when there is none with default settings: error occurred", LogLevel.Error, Times.Exactly(1))
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Never())
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never())
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never())
                .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1));
        }

        [Fact]
        public async Task when_creating_new_versioningtable_fails()
        {
            const string databaseName = "EasyDbMigrator";

            MigrationConfiguration config = new MigrationConfiguration(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var sqlDbHelperMock = new Mock<IDatabaseConnector>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var scriptsHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception())); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , sqlDbHelperMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
               .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1))
               .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed with errors", LogLevel.Error, Times.Exactly(1))
               .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never())
               .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never())
               .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1));
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

            var scriptsHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var sqlDbHelperMock = new Mock<IDatabaseConnector>();
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            _ = sqlDbHelperMock.SetupSequence(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>()))
                .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ExceptionWasThownWhenScriptWasExecuted, exception: new Exception()));

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , sqlDbHelperMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
                  .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was not completed due to exception", LogLevel.Error, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("script: 20212230_003_Script3.sql was skipped due to exception in previous script", LogLevel.Warning, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1));


            //check that the 3rd script is skipped because of the error in the previous script so we check here that max 2 db calls are made
            sqlDbHelperMock.Verify(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                , It.IsAny<Script>()
                , It.IsAny<DateTime>())
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

            var scriptsHelperMock = new Mock<IAssemblyResourceHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConverManifestResourceStreamsToScriptsAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            var sqlDbHelperMock = new Mock<IDatabaseConnector>();
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            _ = sqlDbHelperMock.SetupSequence(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<MigrationConfiguration>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>()))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.ScriptSkippedBecauseAlreadyRun, exception: null))
                    .ReturnsAsync(new Result<RunMigrationResult>(isSucces: true, RunMigrationResult.MigrationScriptExecuted, exception: null));

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , migrationConfiguration: config
                , sqlDbHelperMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
                  .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("Total scripts found: 3", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("script: 20212230_003_Script3.sql was run", LogLevel.Information, Times.Exactly(1))
                  .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1));
        }
    }
}
