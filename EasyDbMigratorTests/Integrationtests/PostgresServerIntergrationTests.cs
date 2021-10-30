using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.Integrationtests.Helpers;
using EasyDbMigratorTests.TestHelpers;
using ExampleTestLibWithPostgreSQLServerScripts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Postgres;
using Xunit;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
    [Collection(nameof(NotRunParallel))]
    public class PostgresServerIntergrationTests
    {
        private const string _databaseName = "EasyDbMigratorPostgresServer";
        private const string _userName = "retrodad";
        private const string _password = "stuffy6!";
        private DockerEnvironment _dockerPostgresServerEnvironment;

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task the_scenario_when_nothing_goes_wrong_with_running_migrations_on_an_empty_database_with_cancellationToken()
        {
            var environmentBuilder = new DockerEnvironmentBuilder();
            _dockerPostgresServerEnvironment = SetupDockerPostgresServerTestEnvironment(environmentBuilder);
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            try
            {
                await _dockerPostgresServerEnvironment.Up().ConfigureAwait(true);
                var connectionString = _dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTime ExecutedDataTime = new DateTime(2021, 10, 17, 12, 10, 10);

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object
                    , databaseConnector: new PostgreSqlConnector());

                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20211230_001_DoStuffScript.sql");
                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

                bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                _ = loggerMock
                    .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211230_002_Script2p.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211231_001_Script1p.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                List<DbMigrationsRunRowPostgressServer> expectedRows = new List<DbMigrationsRunRowPostgressServer>();
                expectedRows.Add(new DbMigrationsRunRowPostgressServer(id: 1, executed: ExecutedDataTime, filename: "20211230_002_Script2p.sql", version: "1.0.0"));
                expectedRows.Add(new DbMigrationsRunRowPostgressServer(id: 2, executed: ExecutedDataTime, filename: "20211231_001_Script1p.sql", version: "1.0.0"));

                _ = IntegrationTestHelper.CheckMigrationsTablePostgresSever(connectionString: connectionString
                  , expectedRows: expectedRows);

            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                source.Dispose();
                _dockerPostgresServerEnvironment.Dispose();
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task the_scenario_when_nothing_goes_wrong_with_running_migrations_on_an_empty_database_without_cancellationToken()
        {
            var environmentBuilder = new DockerEnvironmentBuilder();
            _dockerPostgresServerEnvironment = SetupDockerPostgresServerTestEnvironment(environmentBuilder);

            try
            {
                await _dockerPostgresServerEnvironment.Up().ConfigureAwait(true);
                var connectionString = _dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTime ExecutedDataTime = new DateTime(2021, 10, 17, 12, 10, 10);

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object
                    , databaseConnector: new PostgreSqlConnector());

                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20211230_001_DoStuffScript.sql");
                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

                bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                _ = loggerMock
                    .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211230_002_Script2p.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211231_001_Script1p.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                List<DbMigrationsRunRowPostgressServer> expectedRows = new List<DbMigrationsRunRowPostgressServer>();
                expectedRows.Add(new DbMigrationsRunRowPostgressServer(id: 1, executed: ExecutedDataTime, filename: "20211230_002_Script2p.sql", version: "1.0.0"));
                expectedRows.Add(new DbMigrationsRunRowPostgressServer(id: 2, executed: ExecutedDataTime, filename: "20211231_001_Script1p.sql", version: "1.0.0"));

                _ = IntegrationTestHelper.CheckMigrationsTablePostgresSever(connectionString: connectionString
                  , expectedRows: expectedRows);

            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                _dockerPostgresServerEnvironment.Dispose();
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task the_scenario_when_all_the_migration_script_allready_have_been_executed_before()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            try
            {
                var environmentBuilder = new DockerEnvironmentBuilder();
                _dockerPostgresServerEnvironment = SetupDockerPostgresServerTestEnvironment(environmentBuilder);
                await _dockerPostgresServerEnvironment.Up().ConfigureAwait(true);
                var connectionString = _dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock1 = new Mock<IDataTimeHelper>();
                DateTime ExecutedFirsttimeDataTime = new DateTime(2021, 12, 30, 2, 16, 1);

                _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedFirsttimeDataTime);

                DbMigrator migrator1 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock1.Object
                    , databaseConnector: new PostgreSqlConnector());

                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20211230_001_DoStuffScript.sql");

                migrator1.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

                bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                // now run the migrations again
                var loggerMockSecondtRun = new Mock<ILogger<DbMigrator>>();
                DateTime ExecutedSecondtimeDataTime = new DateTime(2021, 12, 31, 2, 16, 1);

                Mock<IDataTimeHelper> datetimeHelperMock2 = new Mock<IDataTimeHelper>();
                _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedSecondtimeDataTime);

                DbMigrator migrator2 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                   , logger: loggerMockSecondtRun.Object
                   , dataTimeHelperMock: datetimeHelperMock2.Object
                   , databaseConnector: new PostgreSqlConnector());

                migrator2.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeeded = await migrator2.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeeded.Should().BeTrue();

                _ = loggerMockSecondtRun
                    .CheckIfLoggerWasCalled("setup database when there is none with default settings executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup DbMigrationsRun table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211230_002_Script2p.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211231_001_Script1p.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("Whole migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                //  version - table should not be updated for the second time

                List<DbMigrationsRunRowPostgressServer> expectedRows = new List<DbMigrationsRunRowPostgressServer>();
                expectedRows.Add(new DbMigrationsRunRowPostgressServer(id: 1, executed: ExecutedFirsttimeDataTime, filename: "20211230_002_Script2p.sql", version: "1.0.0"));
                expectedRows.Add(new DbMigrationsRunRowPostgressServer(id: 2, executed: ExecutedFirsttimeDataTime, filename: "20211231_001_Script1p.sql", version: "1.0.0"));

                _ = IntegrationTestHelper.CheckMigrationsTablePostgresSever(connectionString: connectionString
                 , expectedRows: expectedRows);

            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                source.Dispose();
                _dockerPostgresServerEnvironment.Dispose();
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task can_cancel_the_migration_just_before_the_scripts_will_run()
        {
            var environmentBuilder = new DockerEnvironmentBuilder();
            _dockerPostgresServerEnvironment = SetupDockerPostgresServerTestEnvironment(environmentBuilder);
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            try
            {
                await _dockerPostgresServerEnvironment.Up().ConfigureAwait(true);
                var connectionString = _dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();
                DateTime ExecutedDataTime = new DateTime(2021, 10, 17, 12, 10, 10);

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object
                    , databaseConnector: new PostgreSqlConnector());

                List<string> scriptsToExclude = new List<string>();
                scriptsToExclude.Add("20211230_001_DoStuffScript.sql");
                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

                source.Cancel();

                bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                _ = loggerMock
                     .CheckIfLoggerWasCalled("Whole migration process was canceled", LogLevel.Warning, Times.Exactly(1), checkExceptionNotNull: false);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                source.Dispose();
                _dockerPostgresServerEnvironment.Dispose();
            }
        }

        private DockerEnvironment SetupDockerPostgresServerTestEnvironment(DockerEnvironmentBuilder environmentBuilder)
        {
            if (_dockerPostgresServerEnvironment != null)
                return _dockerPostgresServerEnvironment;

            return environmentBuilder.UseDefaultNetwork()
                .SetName("xunit-EasyDbMigratorPostgresServer")
                //pick for now the latest version of sqlserver (= default)
                .AddPostgresContainer(name: _databaseName
                    , userName: _userName
                    , password: _password)
                .Build();
        }
    }
}
