# The Latest news and Roadmap

## 4.0.x (the latest release)
- add support for .net 9 and removed the not supported .net versions
- is AOT compatible

! from version 4.0.2 on EasyDBMigrator will not use the Fluent Assertions package for internal testing. The reason is that Fluent Assertions is a commercial product now. EasyDBMigration will only use non-commercial opensource software.

## 3.0.x (the latest release)
- add support for .net 8 and
- monthly updates of all packages for stability and security

## 2.0.x
- add support for .net 7
- monthly update of all packages for stability and security

# INFO:

### Action (jobs):
[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildTestDebug.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildTestDebug.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildRelease.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildRelease.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/PackageAndReleaseMasterToNuGet.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/PackageAndReleaseMasterToNuGet.yml)

[![Build](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql.yml)

### Code statistics:
[![codecov](https://codecov.io/gh/Retrodad0001/easydbmigrator/branch/master/graph/badge.svg?token=JWYWLP98IW)](https://codecov.io/gh/Retrodad0001/easydbmigrator)

### More info:

[![NuGet](https://img.shields.io/nuget/v/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/) 
[![Nuget](https://img.shields.io/nuget/dt/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/)


# EasyDBMigrator - making database migrations and integration testing easier

## What is EasyDBMigrator?

EasyDBMigrator is an open-source database migration framework. It strongly favors simplicity and ease of use for automatic CI/CD strategies, integration testing scenarios and easy integration in your own code.
  
## What do we want to accomplish?

1. Make it easy to integrate Microsoft SQL migrations in your CI/CD flows
2. Make it easy to integrate Postgre SQL migrations in your CI/CD flows
5. Make it easy to perform manual Microsoft SQL migrations
6. Make it easy to perform manual Postgre SQL migrations
7. Make it easy to integrate this package into your own code/tool
8. Provide examples of how to write unit and integration tests (feel free to give me code feedback or for a good discussion on testing)

## Get started with EasyDBMigrator :

### using easyDbMigrator for integration testing:

        private async Task RunMigrations()
        {
            //Make sure you use the correct naming in your scripts, like:
            // 20210926_001_AddEquipmentTable.sql --> script are ordered by date and then per sequence number. In this case, '001' is the sequence number.
            //Make sure to set the BUILD-ACTION property of every migration SQL script to EMBEDDED RESOURCE
            const string databaseName = "WorkoutIntegrationTests";
            
            //Make sure that the 'Database =  xxxx;' parameter is excluded in the connection-string
            const string connectionstring = "some fancy connectionstring without database param";
            MigrationConfiguration config = new MigrationConfiguration(connectionString: connectionstring, databaseName: databaseName);

            //for writing the logging output from EasyDBMigrator to the test explorer output window
            var logger = XUnitLoghelper.CreateLogger<DatabaseTests>(_testOutputHelper);
            Mock<IDataTimeHelper> datetimeHelperMock = new Mock<IDateTimeHelper>();

            DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(migrationConfiguration: config
                  , logger: loggerMock.Object
                  , dateTimeHelperMock: datetimeHelperMock.Object
                  , databaseConnector: new MicrosoftSqlConnector()); 
            //can also use the PostgreSqlConnector to connect to PostgreSQL instead of Microsoft Sql Server

            bool succeededDeletingDatabase = await migrator.TryDeleteDatabaseIfExistAsync(migrationConfiguration: config
                    , cancellationToken: token);
                _ = succeededDeletingDatabase.Should().BeTrue();

            bool succeededRunningMigrations = await migrator.TryApplyMigrationsAsync(typeOfClassWhereScriptsAreLocated: typeof(HereTheSQLServerScriptsCanBeFound)
                    , migrationConfiguration: config
                    , cancellationToken: token);
                _ = succeededRunningMigrations.Should().BeTrue();
            
            //download the code if you want to see examples of integration testing with easyDbMigrator
        }

## FAQ


## Alternatives
https://github.com/erikbra/grate

https://github.com/fluentmigrator/fluentmigrator

## License
Free to use
