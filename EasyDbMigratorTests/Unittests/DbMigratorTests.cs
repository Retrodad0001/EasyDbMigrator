using EasyDbMigrator;
using EasyDbMigrator.Helpers;
using EasyDbMigrator.Infra;
using EasyDbMigratorTests.Integrationtests.TestHelpers;
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
        public async Task when_everything_goes_ok()
        {
            const string databaseName = "EasyDbMigrator";

            SqlDataBaseInfo sqlDataBaseInfo = new SqlDataBaseInfo(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var sqlDbHelperMock = new Mock<ISqlDbHelper>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var scriptsHelperMock = new Mock<IScriptsHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConvertoScriptsInCorrectSequenceByTypeAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<SqlDataBaseInfo>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , sqlDbHelperMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
                .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), shouldLogExeption: false)
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1), shouldLogExeption: false)
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), shouldLogExeption: false)
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), shouldLogExeption: false)
                .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), shouldLogExeption: false);

        }

        [Fact]
        public async Task when_creating_new_database_fails()
        {
            const string databaseName = "EasyDbMigrator";

            SqlDataBaseInfo sqlDataBaseInfo = new SqlDataBaseInfo(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var sqlDbHelperMock = new Mock<ISqlDbHelper>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var scriptsHelperMock = new Mock<IScriptsHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConvertoScriptsInCorrectSequenceByTypeAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null)); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<SqlDataBaseInfo>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , sqlDbHelperMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
                .CheckIfLoggerWasCalled("setup database when there is none with default settings: error occurred", LogLevel.Error, Times.Exactly(1), shouldLogExeption: true)
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Never(), shouldLogExeption: false)
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), shouldLogExeption: false)
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never(), shouldLogExeption: false)
                .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1), shouldLogExeption: false);
        }

        [Fact]
        public async Task when_creating_new_versioningtable_fails()
        {
            const string databaseName = "EasyDbMigrator";

            SqlDataBaseInfo sqlDataBaseInfo = new SqlDataBaseInfo(connectionString: "connection"
                , databaseName: databaseName);

            Type someType = typeof(DbMigratorTests);

            var loggerMock = new Mock<ILogger<DbMigrator>>();
            var sqlDbHelperMock = new Mock<ISqlDbHelper>();

            Script script1 = new Script("20212230_001_Script1.sql", "some content");
            Script script2 = new Script("20212230_002_Script2.sql", "some content");
            List<Script> scripts = new List<Script>();
            scripts.Add(script1);
            scripts.Add(script2);

            var scriptsHelperMock = new Mock<IScriptsHelper>();
            _ = scriptsHelperMock.Setup(m => m.TryConvertoScriptsInCorrectSequenceByTypeAsync(someType)).Returns(() => Task.FromResult<List<Script>>(scripts));

            Result<bool> result = new Result<bool>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.SetupSequence(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()))
                    .ReturnsAsync(new Result<bool>(isSucces: true, exception: null))
                    .ReturnsAsync(new Result<bool>(isSucces: false, exception: new Exception())); //second time this method is called
            ;

            Result<RunMigrationResult> resultRunMigrations = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.Setup(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<SqlDataBaseInfo>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>())).ReturnsAsync(resultRunMigrations);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object
                , sqlDbHelperMock.Object
                , scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
               .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), shouldLogExeption: false)
               .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed with errors", LogLevel.Error, Times.Exactly(1), shouldLogExeption: true)
               .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Never(), shouldLogExeption: false)
               .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Never(), shouldLogExeption: false)
               .CheckIfLoggerWasCalled("Whole migration process executed with errors", LogLevel.Error, Times.Exactly(1), shouldLogExeption: false);
        }

        //TODO HIGH: test when 2e script fails then stop running other scripts and do some useful logging
        //TODO HIGH: test when script already run don't execute and log stuff       
       
        //TODO LOW: test ignore non .sql files
    }
}
