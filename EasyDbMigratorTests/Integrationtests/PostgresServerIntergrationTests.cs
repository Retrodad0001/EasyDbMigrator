using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.Integrationtests.Helpers;
using EasyDbMigratorTests.TestHelpers;
using ExampleTestLibWithPostGreSQLServerScripts;
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

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task when_nothing_goes_wrong_with_running_all_migrations_on_an_empty_database()
        {
            var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment();

            try
            {
                await dockerPostgresServerEnvironment.UpAsync().ConfigureAwait(true);
                var connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock = new();
                DateTime ExecutedDataTime = new(2021, 10, 17, 12, 10, 10);

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object
                    , databaseConnector: new PostgreSqlConnector());

                List<string> scriptsToExclude = new();
                scriptsToExclude.Add("20211230_001_DoStuffScript.sql");
                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

                bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

                bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                _ = loggerMock
                    .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211230_002_Script2p.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211231_001_Script1p.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                List<DbMigrationsRunRowPostgressServer> expectedRows = new();
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
                await dockerPostgresServerEnvironment.DisposeAsync().ConfigureAwait(true);
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task can_skip_scripts_if_they_allready_run_before()
        {
            var dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment();

            try
            {
                await dockerPostgresServerEnvironment.UpAsync().ConfigureAwait(true);
                var connectionString = dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock1 = new();
                DateTime ExecutedFirsttimeDataTime = new(2021, 12, 30, 2, 16, 1);

                _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedFirsttimeDataTime);

                DbMigrator migrator1 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock1.Object
                    , databaseConnector: new PostgreSqlConnector());

                List<string> scriptsToExclude = new();
                scriptsToExclude.Add("20211230_001_DoStuffScript.sql");

                migrator1.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

                bool succeededDeleDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                var type = typeof(HereThePostgreSQLServerScriptsCanBeFound);

                bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                // now run the migrations again
                var loggerMockSecondtRun = new Mock<ILogger<DbMigrator>>();
                DateTime ExecutedSecondtimeDataTime = new(2021, 12, 31, 2, 16, 1);

                Mock<IDataTimeHelper> datetimeHelperMock2 = new();
                _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedSecondtimeDataTime);

                DbMigrator migrator2 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                   , logger: loggerMockSecondtRun.Object
                   , dataTimeHelperMock: datetimeHelperMock2.Object
                   , databaseConnector: new PostgreSqlConnector());

                migrator2.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

                bool succeeded = await migrator2.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeeded.Should().BeTrue();

                _ = loggerMockSecondtRun
                    .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211230_002_Script2p.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211231_001_Script1p.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                //  version - table should not be updated for the second time
                List<DbMigrationsRunRowPostgressServer> expectedRows = new();
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
                await dockerPostgresServerEnvironment.DisposeAsync().ConfigureAwait(true);
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task can_cancel_the_migration_proces()
        {

            var _dockerPostgresServerEnvironment = SetupPostgresServerTestEnvironment();
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;

            try
            {
                await _dockerPostgresServerEnvironment.UpAsync().ConfigureAwait(true);
                var connectionString = _dockerPostgresServerEnvironment.GetContainer<PostgresContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new(connectionString: connectionString
                    , databaseName: _databaseName);

                var loggerMock = new Mock<ILogger<DbMigrator>>();

                Mock<IDataTimeHelper> datetimeHelperMock = new();
                DateTime ExecutedDataTime = new(2021, 10, 17, 12, 10, 10);

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object
                    , databaseConnector: new PostgreSqlConnector());

                List<string> scriptsToExclude = new();
                scriptsToExclude.Add("20211230_001_DoStuffScript.sql");
                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByName: scriptsToExclude);

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
                     .CheckIfLoggerWasCalled("migration process was canceled from the outside", LogLevel.Warning, Times.Exactly(1), checkExceptionNotNull: false);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                source.Dispose();
                await _dockerPostgresServerEnvironment.DisposeAsync().ConfigureAwait(true);
            }
        }

        private static DockerEnvironment SetupPostgresServerTestEnvironment()
        {
            var environmentBuilder = new DockerEnvironmentBuilder();

            const string _userName = "retrodad";
            const string _password = "stuffy6!";

            return (DockerEnvironment)environmentBuilder
                 .SetName(_databaseName)
                 .AddPostgresContainer(p =>
                 {
                     return p with
                     {
                         Name = _databaseName,
                         UserName = _userName,
                         Password = _password
                     };
                 }).Build();
        }
    }
}
