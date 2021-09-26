# Latest news 
The software can only be used for integrationtesting. Running manual migrations and using it in CI/CD pipelines is under-construction

# STATISTICS:

## Builds:
[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/CI.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/CI.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/Deploy.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/Deploy.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis.yml)

[![codecov](https://codecov.io/gh/Retrodad0001/easydbmigrator/branch/master/graph/badge.svg?token=JWYWLP98IW)](https://codecov.io/gh/Retrodad0001/easydbmigrator)

|         |       |       |        |
| ------- | ----- | ----- | -----  |
| `EasyDbMigrator` | core & integration testing |[![NuGet](https://img.shields.io/nuget/v/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/) | [![Nuget](https://img.shields.io/nuget/dt/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/) |
| `EasyDbMigrator runner` | running migrations manual and CD pipelines support  | TODO | TODO |

# EasyDBMigrator - making database migrations easier

## What is EasyDBMigrator?

EasyDBMigrator is an open-source sql database migration framework & Tool. It strongly favors simplicity and easy to use for automatic CI/CD strategies and local integration testing scenario's. 

Migrations can be written in SQL (T-SQL). 

It has a Command-line client for managing migrations and a framework written for .net to integrate database migrations in local en CI pipeline based integration testing scenario's.
    
## Wat we want to accomplish ?

1. Make it easy to integrate SQL migrations in your local integration tests (written in .NET)
2. Make it easy to integrate SQL migrations in your CI/CD flows (runner)
3. Make it easy to perform manual SQL migration

## Get started with EasyDBMigrator :

### using easyDbMigrator for integration testing:

        private async Task RunMigrations()
        {
            //Make sure u use the correct naming in your scripts like:
            // 20210926_001_AddEquipmentTable.sql --> script are ordered by date and then per sequence number. in this case '001' is the sequence number
            //Make sure to set the BUILD-ACTION property of every migration sql script to EMBEDDED RESOURCE
            const string databaseame = "WorkoutIntegrationTests";
            
            //Make sure that the 'Database =  xxxx;' param is excluded in the connectionstring 
            const string connectionstring = "some fancy connectionstring without database param";
            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring
                , databaseName: databaseame);

            //handy until for writing the logging output from EasyDbMigratior to the xUnit output window
            //U don't need to use this trick and just mock out the ILogger when u don't want to use this or when u use something else than xunit
            var logger = XUnitLoghelper.CreateLogger<DatabaseTests>(_testOutputHelper);
            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();

            DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                , logger: logger
                , dataTimeHelperMock: datetimeHelperMock.Object);

            bool succesDeleteDatabase = await migrator.TryDeleteDatabaseIfExistAsync(databaseName: databaseame, connectionString: _connectionStringForDBMigrator);
            _ = succesDeleteDatabase.Should().BeTrue();
            
            bool succesApplyMigrations = await migrator.TryApplyMigrationsAsync(typeof(MigrationLocation));
            _ = succesApplyMigrations.Should().BeTrue();

            //download the code if u want to see some real examples or integration testing with easyDbMigrator
        }

## FAQ


## Alternatives
https://github.com/chucknorris/roundhouse

https://github.com/fluentmigrator/fluentmigrator

## License
Free to use
