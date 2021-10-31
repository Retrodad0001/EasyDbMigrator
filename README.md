# The Latest news 

## working on milestone: 1.3.0 (next release)
- [busy] can customize the script sequence pattern so i can use it with my current scripts 
    examples: 11112233-444-555.sql, 111122334444.sql, 111122.sql, 1111_2222.sql or whatever
- [busy] support for .net 6 LTS
- [busy] updated all internal packages

## 1.2.0 (the latest release):
- can use a file directory for scripts
- speedup the inner development loop by getting the docker-image and running it in the integration tests (with examples)
- can mock EasyDbMigrator when u want to integrate it in your own code
- can use DbMigrator in .net Dependency injection (IServiceCollection)
- can cancel the process from outside
- make executions and configuration more resilient
- support PostgreSQL migrations in your integration test
- bugfixes and better logging
- updated all internal packages
 
## 1.1.0
 - support for .net core 3.1 (until LTS ends) and .net 5 (until support ends)
 - updated .net packages
 - updated external packages
 - updated to latest .net 3.1.x and 5.0.x with security updates

The software can only be used for integration-testing. Running manual migrations and using it in CI/CD pipelines is under-construction. 
It is possible to use EasyDbMigrator in your own application.

# INFO:

### Builds:
[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/CD.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/CD.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis-weekly.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis-weekly.yml)

### Code statistics:
[![codecov](https://codecov.io/gh/Retrodad0001/easydbmigrator/branch/master/graph/badge.svg?token=JWYWLP98IW)](https://codecov.io/gh/Retrodad0001/easydbmigrator)

### More info:

|         |       |       |        |
| ------- | ----- | ----- | -----  |
| `EasyDbMigrator` | core & integration testing |[![NuGet](https://img.shields.io/nuget/v/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/) | [![Nuget](https://img.shields.io/nuget/dt/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/) |
| `EasyDbMigrator runner` | running migrations manual and CD pipelines support  | TODO | TODO |

# EasyDBMigrator - making database migrations and integration testing easier

## What is EasyDBMigrator?

EasyDBMigrator is an open-source SQL database migration framework & Tool. It strongly favors simplicity and easy to use for automatic CI/CD strategies and local integration testing scenario's. 

It has a Command-line client for managing migrations and a framework written for .net to integrate database migrations in local en CI pipeline based integration testing scenario's.
    
## What we want to accomplish ?

1. Make it easy to integrate Microsoft SQL migrations in your local integration testsuite
2. Make it easy to integrate Postgre SQL migrations in your local integration testsuite
3. Make it easy to integrate Microsoft SQL migrations in your CI/CD flows
4. Make it easy to integrate Postgre SQL migrations in your CI/CD flows
5. Make it easy to perform manual Microsoft SQL migrations
6. Make it easy to perform manual Postgre SQL migrations
7. Provide examples and stimulate discussions about writing integration tests in code (so please send me feedback or start a good discussion)

## Get started with EasyDBMigrator :

### using easyDbMigrator for integration testing:

        private async Task RunMigrations()
        {
            //int the code u find some more advanced examples including setting up docker containers automatically in code

            //Make sure u use the correct naming in your scripts like:
            // 20210926_001_AddEquipmentTable.sql --> script are ordered by date and then per sequence number. in this case '001' is the sequence number
            //Make sure to set the BUILD-ACTION property of every migration sql script to EMBEDDED RESOURCE
            const string databaseame = "WorkoutIntegrationTests";
            
            //Make sure that the 'Database =  xxxx;' param is excluded in the connection-string
            const string connectionstring = "some fancy connectionstring without database param";
            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring, databaseName: databaseName);

            //handy until for writing the logging output from EasyDbMigratior to the xUnit output window
            //U don't need to use this trick and just mock out the ILogger when u don't want to use this or when u use something else than xunit
            var logger = XUnitLoghelper.CreateLogger<DatabaseTests>(_testOutputHelper);
            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDataTimeHelper>();

            DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                  , logger: loggerMock.Object
                  , dataTimeHelperMock: datetimeHelperMock.Object
                  , databaseConnector: new MicrosoftSqlConnector()); 
            //can also use the PostgreSqlConnector to connect to PostgreSql instead of Microsoft Sql Server

            bool succeededDeleDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: token);
                _ = succeededDeleDatabase.Should().BeTrue();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereTheSQLServerScriptsCanBeFound)
                    , migrationConfiguration: config
                    , cancellationToken: token);
                _ = succeededRunningMigrations.Should().BeTrue();
            
            //download the code if u want to see examples of integration testing with easyDbMigrator
        }

## FAQ


## Alternatives
https://github.com/chucknorris/roundhouse

https://github.com/erikbra/grate

https://github.com/fluentmigrator/fluentmigrator

## License
Free to use
