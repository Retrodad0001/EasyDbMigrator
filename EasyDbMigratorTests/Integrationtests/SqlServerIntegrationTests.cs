using EasyDbMigrator;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EasyDbMigratorTests.Integrationtests.TestHelpers;
using FluentAssertions;
using ExampleTestLibWithScripts;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Mssql;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
    [Collection(nameof(classNotRunParallel))]
    public class SqlServerIntegrationTests
    {
        private const string _databaseName = "EasyDbMigrator";
        private const string _password = "stuffy6!";
        private readonly IDictionary<ushort, ushort> _ports = new Dictionary<ushort, ushort> ();
        private DockerEnvironment _dockerEnvironment;//TODO when null create container

        private DockerEnvironment SetupDockerTestEnvironment(DockerEnvironmentBuilder environmentBuilder)
        {
            if (_dockerEnvironment != null)
                return _dockerEnvironment;

            _ports.Add(1433, 1433);
            return environmentBuilder.UseDefaultNetwork()
                .SetName("xunit-EasydbMigratorIntegrationTest")
                //pick for now the latest version of sqlserver (= default)
                .AddMssqlContainer(name: _databaseName, saPassword: _password, ports: _ports)
                .Build();
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task the_scenario_when_nothing_goes_wrong_with_running_migrations_on_an_empty_database()
        {
            var environmentBuilder = new DockerEnvironmentBuilder();
            _dockerEnvironment = SetupDockerTestEnvironment(environmentBuilder);

            try
            {
                await _dockerEnvironment.Up();
                var connectionString = _dockerEnvironment.GetContainer<MssqlContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedDataTime = DateTime.UtcNow;

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object);

                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20212230_001_CreateDB.sql");
                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(databaseName: _databaseName, connectionString: connectionString);
                _ = succeededDeleDatabase.Should().BeTrue();

                bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereScriptsCanBeFound));
                _ = succeededRunningMigrations.Should().BeTrue();

                _ = loggerMock
                    .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212231_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                List<DbMigrationsRunRow> expectedRows = new List<DbMigrationsRunRow>();
                expectedRows.Add(new DbMigrationsRunRow(id: 1, executed: ExecutedDataTime, filename: "20212230_002_Script2.sql", version: "1.0.0"));
                expectedRows.Add(new DbMigrationsRunRow(id: 2, executed: ExecutedDataTime, filename: "20212231_001_Script1.sql", version: "1.0.0"));

                _ = new DbTestHelper().CheckMigrationsTable(connectionString: connectionString
                  , expectedRows: expectedRows
                  , testdbName: _databaseName);

            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                await _dockerEnvironment.DisposeAsync();
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task the_scenario_when_all_the_migration_script_allready_have_been_executed_before()
        {
            try
            {
                var environmentBuilder = new DockerEnvironmentBuilder();
                _dockerEnvironment = SetupDockerTestEnvironment(environmentBuilder);
                await _dockerEnvironment.Up();
                var connectionString = _dockerEnvironment.GetContainer<MssqlContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock1 = new Mock<IDataTimeHelper>();
                DateTimeOffset ExecutedFirsttimeDataTime = DateTimeOffset.UtcNow;

                _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedFirsttimeDataTime);

                DbMigrator migrator1 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock1.Object);

                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20212230_001_CreateDB.sql");

                migrator1.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(databaseName: _databaseName, connectionString: connectionString);
                _ = succeededDeleDatabase.Should().BeTrue();

                bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereScriptsCanBeFound));
                _ = succeededRunningMigrations.Should().BeTrue();

                //now run the migrations again
                var loggerMockSecondtRun = new Mock<ILogger<DbMigrator>>();
                DateTime ExecutedSecondtimeDataTime = new DateTime(2021, 12, 31, 2, 16, 1);

                Mock<IDataTimeHelper> datetimeHelperMock2 = new Mock<IDataTimeHelper>();
                _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedSecondtimeDataTime);

                DbMigrator migrator2 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                   , logger: loggerMockSecondtRun.Object
                   , dataTimeHelperMock: datetimeHelperMock2.Object);

                migrator2.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeeded = await migrator2.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereScriptsCanBeFound));
                _ = succeeded.Should().BeTrue();

                _ = loggerMockSecondtRun
                    .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup DbMigrationsRun when there is none executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20212231_001_Script1.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                //version-table should not be updated for the second time
                List<DbMigrationsRunRow> expectedRows = new List<DbMigrationsRunRow>();
                expectedRows.Add(new DbMigrationsRunRow(id: 1, executed: ExecutedFirsttimeDataTime, filename: "20212230_002_Script2.sql", version: "1.0.0"));
                expectedRows.Add(new DbMigrationsRunRow(id: 2, executed: ExecutedFirsttimeDataTime, filename: "20212231_001_Script1.sql", version: "1.0.0"));

                _ = new DbTestHelper().CheckMigrationsTable(connectionString: connectionString
                 , expectedRows: expectedRows
                 , testdbName: _databaseName);

            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                await _dockerEnvironment.DisposeAsync();
            }
        }
    }
}
