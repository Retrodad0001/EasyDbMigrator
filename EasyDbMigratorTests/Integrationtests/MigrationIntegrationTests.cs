using EasyDbMigrator;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TestLib;
using Xunit;
using Moq;
using EasyDbMigratorTests.Integrationtests.TestHelpers;
using FluentAssertions;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
    [CollectionDefinition(nameof(classNotRunParallel), DisableParallelization = true)]
    public class classNotRunParallel { }

    [ExcludeFromCodeCoverage]
    [Collection(nameof(classNotRunParallel))]
    public class MigrationIntegrationTests
    {



        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task the_scenario_when_nothing_goes_wrong_with_running_migrations_on_an_empty_database()
        {
            try
            {
                const string connectionstring = @"Data Source = localhost,1433; User ID = sa; Password=stuffy666!; Connect Timeout = 30; Encrypt=False; TrustServerCertificate=False; ApplicationIntent=ReadWrite; MultiSubnetFailover=False";
                const string databaseName = "EasyDbMigrator";

                await DeleteDatabaseIfExistAsync(databaseName: databaseName, connectionString: connectionstring);

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring
                    , databaseName: databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                
                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object);
               
                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20212230_001_CreateDB.sql");
                migrator.ExcludeScripts(scriptsToExclude);
                
                bool succeeded = await migrator.TryApplyMigrationsAsync(customClass: typeof(SomeCustomClass));

                _ = succeeded.Should().BeTrue();

                _ = loggerMock
                    .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212231_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                List<VersioningTableRow> expectedRows = new List<VersioningTableRow>();
                expectedRows.Add(new VersioningTableRow(id: 1, executed: ExecutedDataTime, filename: "20212230_002_Script2.sql", version: "1.0.0"));
                expectedRows.Add(new VersioningTableRow(id: 2, executed: ExecutedDataTime, filename: "20212231_001_Script1.sql", version: "1.0.0"));

                _ = new DbTestHelper().CheckMigrationsTable(connectionString: connectionstring
                  , expectedRows: expectedRows
                  , testdbName: databaseName);

            }
#pragma warning disable CA1031 // Do not catch general exception types, for sake of testing this is no problem
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Assert.True(false, ex.ToString());
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task the_scenario_when_all_the_migration_script_allready_have_been_executed_before()
        {
            try
            {
                const string connectionstring = @"Data Source = localhost,1433; User ID = sa; Password=stuffy666!; Connect Timeout = 30; Encrypt=False; TrustServerCertificate=False; ApplicationIntent=ReadWrite; MultiSubnetFailover=False";
                const string databaseName = "EasyDbMigrator";

                await DeleteDatabaseIfExistAsync(databaseName: databaseName, connectionString: connectionstring);

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring
                    , databaseName: databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock1 = new Mock<IDataTimeHelper>();
                DateTime ExecutedFirsttimeDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

                _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedFirsttimeDataTime);

                DbMigrator migrator1 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock1.Object);

                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20212230_001_CreateDB.sql");

                migrator1.ExcludeScripts(scriptsToExclude);

                _ = await migrator1.TryApplyMigrationsAsync(customClass: typeof(SomeCustomClass));

                //now run the migrations again
                var loggerMockSecondtRun = new Mock<ILogger<DbMigrator>>();
                DateTime ExecutedSecondtimeDataTime = new DateTime(2021, 12, 31, 2, 16, 1);

                Mock<IDataTimeHelper> datetimeHelperMock2 = new Mock<IDataTimeHelper>();
                _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedSecondtimeDataTime);

                DbMigrator migrator2 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                   , logger: loggerMockSecondtRun.Object
                   , dataTimeHelperMock: datetimeHelperMock2.Object);

                migrator2.ExcludeScripts(scriptsToExclude);
                _ = await migrator2.TryApplyMigrationsAsync(customClass: typeof(SomeCustomClass));

                _ = loggerMockSecondtRun
                    .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212231_001_Script1.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                //version-table should not be updated for the second time
                List<VersioningTableRow> expectedRows = new List<VersioningTableRow>();
                expectedRows.Add(new VersioningTableRow(id: 1, executed: ExecutedFirsttimeDataTime, filename: "20212230_002_Script2.sql", version: "1.0.0"));
                expectedRows.Add(new VersioningTableRow(id: 2, executed: ExecutedFirsttimeDataTime, filename: "20212231_001_Script1.sql", version: "1.0.0"));

                _ = new DbTestHelper().CheckMigrationsTable(connectionString: connectionstring
                 , expectedRows: expectedRows
                 , testdbName: databaseName);

            }
#pragma warning disable CA1031 // Do not catch general exception types, for sake of testing this is no problem
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Assert.True(false, ex.ToString());
            }
        }

        private async Task DeleteDatabaseIfExistAsync(string databaseName, string connectionString)
        {
            string query = $@"
                IF EXISTS(SELECT * FROM master.sys.databases WHERE name='{databaseName}')
                BEGIN               
                    ALTER DATABASE {databaseName} 
                    SET OFFLINE WITH ROLLBACK IMMEDIATE;
                    ALTER DATABASE {databaseName} SET ONLINE;
                    DROP DATABASE {databaseName};
                END
                ";
            _ = await new SqlDbConnector().TryExcecuteSingleScriptAsync(connectionString: connectionString
                , scriptName: "EasyDbMigrator.Integrationtest_dropDatabase"
                , sqlScriptContent: query);
        }

    }

}
