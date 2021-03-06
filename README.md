# The Latest news and Roadmap

## (BUSY) Next Release (2.0.0)
- Create fancy cross-platform UI so migrations can be run manual (in .Net MAUI when MAUI released stable version)
- Run production scripts against multiple databases (parallel)

## 1.4.0 (the latest release)
- updated all internal packages for stability and security (i do this at least every month)
- only support for .net 6.0 LTS what is needed for future development (remove .net 5 and .net core 3.1 LTS support)

## 1.3.0 
- support for .net 6 LTS
- updated all internal packages for stability and security

## 1.2.0:
- can point to a directory for scripts
- speedup the inner development loop by getting the docker-image and running it in the integration tests (with examples)
- can mock EasyDbMigrator when used in tests
- can use DbMigrator with .net Dependency injection (IServiceCollection)
- can cancel the process from outside (CancellationToken)
- make executions more resilient (retries)
- support PostgreSQL migrations in your integration test
- bugfixes and better logging
- updated all internal packages
 
## 1.1.0
 - support for .net core 3.1 LTS and .net 5
 - updated .net packages
 - updated all internal packages
 - updated to latest .net 3.1.x and 5.0.x with security updates

The software can only be used for integration-testing. Running manual migrations and using it in CI/CD pipelines is under-construction. 
It is possible to use EasyDbMigrator in your own application.

# INFO:

### Action (jobs):
[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildTestDebug.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildTestDebug.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildRelease.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildRelease.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/PackageAndReleaseMasterToNuGet.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/PackageAndReleaseMasterToNuGet.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis-weekly.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql-analysis-weekly.yml)

### Code statistics:
[![codecov](https://codecov.io/gh/Retrodad0001/easydbmigrator/branch/master/graph/badge.svg?token=JWYWLP98IW)](https://codecov.io/gh/Retrodad0001/easydbmigrator)

### More info:

|[![NuGet](https://img.shields.io/nuget/v/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/) 
[![Nuget](https://img.shields.io/nuget/dt/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/)


# EasyDBMigrator - making database migrations and integration testing easier

## What is EasyDBMigrator?

EasyDBMigrator is an open-source database migration framework. It strongly favors simplicity and easy to use for automatic CI/CD strategies and local integration testing scenario's and easy integration in own code.
  
## What we want to accomplish ?

1. Make it easy to integrate Microsoft SQL migrations in your local integration testsuite
2. Make it easy to integrate Postgre SQL migrations in your local integration testsuite
3. Make it easy to integrate Microsoft SQL migrations in your CI/CD flows
4. Make it easy to integrate Postgre SQL migrations in your CI/CD flows
5. Make it easy to perform manual Microsoft SQL migrations
6. Make it easy to perform manual Postgre SQL migrations
7. Make it easy to integrate this package in your own code/tool
8. Provide examples and stimulate discussions about writing integration tests in code (so please send me feedback or start a good discussion)

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

            bool succeededDeletingDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: token);
                _ = succeededDeletingDatabase.Should().BeTrue();

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
