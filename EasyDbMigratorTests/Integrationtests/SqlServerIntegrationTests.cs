using EasyDbMigrator;
using EasyDbMigrator.DatabaseConnectors;
using EasyDbMigratorTests.Integrationtests.Helpers;
using EasyDbMigratorTests.TestHelpers;
using ExampleTestLibWithSqlServerScripts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Mssql;
using Xunit;

namespace EasyDbMigratorTests.Integrationtests
{
    [ExcludeFromCodeCoverage]
    [Collection(nameof(NotRunParallel))]
    public class SqlServerIntegrationTests
    {
        private const string _databaseName = "EasyDbMigratorSqlServer";

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task when_nothing_goes_wrong_with_running_the_migrations_on_an_empty_database()
        {
            var dockerEnvironmentSql = SetupSQLDockerTestEnvironment();

            try
            {
                await dockerEnvironmentSql.UpAsync().ConfigureAwait(true);
                string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new(connectionString: connectionString
                    , databaseName: _databaseName);

                Mock<ILogger<DbMigrator>> loggerMock = new();

                Mock<IDataTimeHelper> datetimeHelperMock = new();
                DateTimeOffset ExecutedDataTime = DateTime.UtcNow;

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object
                    , databaseConnector: new MicrosoftSqlConnector());

                List<string> scriptsToExclude = new()
                {
                    "20211230_001_CreateDB.sql"
                };

                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereTheSQLServerScriptsCanBeFound)
                    , migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                _ = loggerMock
                    .CheckIfLoggerWasCalled("DeleteDatabaseIfExistAsync has executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was run", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                List<DbMigrationsRunRowSqlServer> expectedRows = new()
                {
                    new DbMigrationsRunRowSqlServer(id: 1, executed: ExecutedDataTime, filename: "20211230_002_Script2.sql", version: "1.0.0"),
                    new DbMigrationsRunRowSqlServer(id: 2, executed: ExecutedDataTime, filename: "20211231_001_Script1.sql", version: "1.0.0")
                };

                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString: connectionString
                  , expectedRows: expectedRows
                  , testDatabaseName: _databaseName);

            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                await dockerEnvironmentSql.DisposeAsync().ConfigureAwait(true);
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task can_skip_scripts_if_they_allready_run_before()
        {
            var dockerEnvironmentSql = SetupSQLDockerTestEnvironment();

            try
            {
                await dockerEnvironmentSql.UpAsync().ConfigureAwait(true);
                string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new(connectionString: connectionString
                    , databaseName: _databaseName);

                Mock<ILogger<DbMigrator>> loggerMock = new();

                Mock<IDataTimeHelper> datetimeHelperMock1 = new();
                DateTimeOffset ExecutedFirsttimeDataTime = DateTimeOffset.UtcNow;

                _ = datetimeHelperMock1.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedFirsttimeDataTime);

                DbMigrator migrator1 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock1.Object
                    , databaseConnector: new MicrosoftSqlConnector());

                List<string> scriptsToExclude = new();
                scriptsToExclude.Add("20211230_001_CreateDB.sql");

                migrator1.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator1.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                Type type = typeof(HereTheSQLServerScriptsCanBeFound);

                bool succeededRunningMigrations = await migrator1.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeededRunningMigrations.Should().BeTrue();

                //now run the migrations again
                Mock<ILogger<DbMigrator>> loggerMockSecondtRun = new();
                DateTime ExecutedSecondtimeDataTime = new(2021, 12, 31, 2, 16, 1);

                Mock<IDataTimeHelper> datetimeHelperMock2 = new();
                _ = datetimeHelperMock2.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedSecondtimeDataTime);

                DbMigrator migrator2 = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                   , logger: loggerMockSecondtRun.Object
                   , dataTimeHelperMock: datetimeHelperMock2.Object
                   , databaseConnector: new MicrosoftSqlConnector());

                migrator2.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeeded = await migrator2.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: type
                    , migrationConfiguration: config
                    , cancellationToken: CancellationToken.None).ConfigureAwait(true);
                _ = succeeded.Should().BeTrue();

                _ = loggerMockSecondtRun
                    .CheckIfLoggerWasCalled("setup database executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("setup versioning table executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211230_002_Script2.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("script: 20211231_001_Script1.sql was not run because script was already executed", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false)
                    .CheckIfLoggerWasCalled("migration process executed successfully", LogLevel.Information, Times.Exactly(1), checkExceptionNotNull: false);

                //version - table should not be updated for the second time

                List<DbMigrationsRunRowSqlServer> expectedRows = new();
                expectedRows.Add(new DbMigrationsRunRowSqlServer(id: 1, executed: ExecutedFirsttimeDataTime, filename: "20211230_002_Script2.sql", version: "1.0.0"));
                expectedRows.Add(new DbMigrationsRunRowSqlServer(id: 2, executed: ExecutedFirsttimeDataTime, filename: "20211231_001_Script1.sql", version: "1.0.0"));

                _ = IntegrationTestHelper.CheckMigrationsTableSqlSever(connectionString: connectionString
                , expectedRows: expectedRows
                , testDatabaseName: _databaseName);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.ToString());
            }
            finally
            {
                await dockerEnvironmentSql.DisposeAsync().ConfigureAwait(true);
            }
        }

        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task can_cancel_the_migration_proces()
        {
            var dockerEnvironmentSql = SetupSQLDockerTestEnvironment();

            CancellationTokenSource source = new();
            CancellationToken token = source.Token;

            try
            {
                await dockerEnvironmentSql.UpAsync().ConfigureAwait(true);
                string connectionString = dockerEnvironmentSql.GetContainer<MssqlContainer>(_databaseName).GetConnectionString();

                MigrationConfiguration config = new(connectionString: connectionString
                    , databaseName: _databaseName);

                Mock<ILogger<DbMigrator>> loggerMock = new();

                Mock<IDataTimeHelper> datetimeHelperMock = new();
                DateTimeOffset ExecutedDataTime = DateTime.UtcNow;

                _ = datetimeHelperMock.Setup(x => x.GetCurrentUtcTime()).Returns(ExecutedDataTime);

                DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                    , logger: loggerMock.Object
                    , dataTimeHelperMock: datetimeHelperMock.Object
                    , databaseConnector: new MicrosoftSqlConnector());

                List<string> scriptsToExclude = new()
                {
                    "20212230_001_CreateDB.sql"
                };

                migrator.ExcludeTheseScriptsInRun(scriptsToExcludeByname: scriptsToExclude);

                bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: token).ConfigureAwait(true);
                _ = succeededDeleDatabase.Should().BeTrue();

                source.Cancel();

                bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereTheSQLServerScriptsCanBeFound)
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
                await dockerEnvironmentSql.DisposeAsync().ConfigureAwait(true);
                source.Dispose();
            }
        }

        private static DockerEnvironment SetupSQLDockerTestEnvironment()
        {
            var environmentBuilder = new DockerEnvironmentBuilder();
            const string password = "stuffy6!";

            IDictionary<ushort, ushort> ports = new Dictionary<ushort, ushort>();
            ports.Add(1433, 1433);
            return (DockerEnvironment)environmentBuilder
                 .SetName(_databaseName)
                 .AddMssqlContainer(p =>
                 {
                     return p with
                     {
                         Name = _databaseName,
                         SAPassword = password,
                         Ports = ports
                     };
                 }).Build();
        }
    }
}