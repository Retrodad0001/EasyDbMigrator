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

            Result<RunMigrationResult> result = new Result<RunMigrationResult>(isSucces: true, exception: null);
            _ = sqlDbHelperMock.Setup(
                x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(
                    It.IsAny<SqlDataBaseInfo>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>())).ReturnsAsync(result);

            DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

            DbMigrator migrator = new(logger: loggerMock.Object, sqlDbHelperMock.Object, scriptsHelperMock.Object);

            _ = await migrator.TryApplyMigrationsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , customClass: someType //just pick this class, doesn't matter
                    , executedDateTime: ExecutedDataTime);

            _ = loggerMock
                .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("script: 20212230_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1))
                .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1));

            sqlDbHelperMock.Verify(x => x.TryExcecuteSingleScriptAsync(It.IsAny<string>()
                   , It.IsAny<string>()
                   , It.IsAny<string>()), Times.Exactly(2));

            sqlDbHelperMock.Verify(x => x.RunDbMigrationScriptWhenNotRunnedBeforeAsync(It.IsAny<SqlDataBaseInfo>()
                    , It.IsAny<Script>()
                    , It.IsAny<DateTime>()), Times.Exactly(2));
                

            //todo check call made

        }

        //TODO ***unittest: add test that when update version table fails that migration script should be run at the end and should be logged
    }
}
