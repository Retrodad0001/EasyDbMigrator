# EasyDBMigrator - making database migrations and integration testing easier

## The latest news and roadmap

### 4.1.x (the latest release)

- add support for .net 10

### 4.0.x

- add support for .net 9 and removed the not supported .net versions
- is AOT compatible

> **Note:** from version 4.0.2 on EasyDBMigrator will not use the Fluent
> Assertions package for internal testing. The reason is that Fluent Assertions
> is a commercial product now. EasyDBMigrator will only use non-commercial open
> source software.
>
> **Note:** EasyDBMigrator is tested with MSTest on Microsoft.Testing.Platform.
> This is an internal testing choice only - it puts no constraint on the test
> framework you use in your own project.

### 3.0.x

- add support for .net 8
- monthly updates of all packages for stability and security

### 2.0.x

- add support for .net 7
- monthly update of all packages for stability and security

## Info

### Actions (jobs)

[![Build debug](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildTestDebug.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildTestDebug.yml)
[![Build release](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildRelease.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/BuildRelease.yml)
[![Package and release to NuGet](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/PackageAndReleaseMasterToNuGet.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/PackageAndReleaseMasterToNuGet.yml)
[![CodeQL](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql.yml/badge.svg)](https://github.com/Retrodad0001/EasyDbMigrator/actions/workflows/codeql.yml)

### Code statistics

[![codecov](https://codecov.io/gh/Retrodad0001/easydbmigrator/branch/master/graph/badge.svg?token=JWYWLP98IW)](https://codecov.io/gh/Retrodad0001/easydbmigrator)

### More info

[![NuGet version](https://img.shields.io/nuget/v/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/)
[![NuGet downloads](https://img.shields.io/nuget/dt/Retrodad.EasyDbMigrator.svg)](https://www.nuget.org/packages/Retrodad.EasyDbMigrator/)

## What is EasyDBMigrator?

EasyDBMigrator is an open-source database migration framework. It strongly
favors simplicity and ease of use for automatic CI/CD strategies, integration
testing scenarios and easy integration in your own code.

## What do we want to accomplish?

1. Make it easy to integrate Microsoft SQL migrations in your CI/CD flows
2. Make it easy to integrate PostgreSQL migrations in your CI/CD flows
3. Make it easy to perform manual Microsoft SQL migrations
4. Make it easy to perform manual PostgreSQL migrations
5. Make it easy to integrate this package into your own code/tool
6. Provide examples of how to write unit and integration tests (feel free to
   give me code feedback or for a good discussion on testing)

## Get started with EasyDBMigrator

### Using EasyDBMigrator for integration testing

The example below uses MSTest, which is what EasyDBMigrator itself is tested
with. EasyDBMigrator only depends on `ILogger`, so any test framework works
just as well.

```csharp
[TestClass]
public class DatabaseTests
{
    // MSTest injects this; it gives you the per-test cancellation token.
    public TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task RunMigrations()
    {
        // Make sure you use the correct naming in your scripts, like:
        // 20210926_001_AddEquipmentTable.sql --> scripts are ordered by date
        // and then per sequence number. In this case, '001' is the sequence
        // number. Make sure to set the BUILD-ACTION property of every
        // migration SQL script to EMBEDDED RESOURCE.
        const string databaseName = "WorkoutIntegrationTests";

        // Make sure that the 'Database = xxxx;' parameter is excluded in the
        // connection-string.
        const string connectionString =
            "some fancy connectionstring without database param";

        MigrationConfiguration config = new(connectionString: connectionString
            , databaseName: databaseName);

        // For writing the logging output from EasyDBMigrator to the test
        // output window.
        ILogger<DbMigrator> logger = LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<DbMigrator>();

        Mock<IDateTimeHelper> dateTimeHelperMock = new();

        // Can also use the PostgreSqlConnector to connect to PostgreSQL
        // instead of Microsoft SQL Server.
        DbMigrator migrator = DbMigrator.CreateForLocalIntegrationTesting(
              migrationConfiguration: config
            , logger: logger
            , dataTimeHelperMock: dateTimeHelperMock.Object
            , databaseConnector: new MicrosoftSqlConnector());

        bool succeededDeletingDatabase =
            await migrator.TryDeleteDatabaseIfExistAsync(
                  migrationConfiguration: config
                , cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(succeededDeletingDatabase);

        bool succeededRunningMigrations =
            await migrator.TryApplyMigrationsAsync(
                  typeOfClassWhereScriptsAreLocated:
                      typeof(HereTheSqlServerScriptsCanBeFound)
                , migrationConfiguration: config
                , cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(succeededRunningMigrations);

        // Download the code if you want to see more examples of integration
        // testing with EasyDBMigrator.
    }
}
```

## Alternatives

- [grate](https://github.com/erikbra/grate)
- [FluentMigrator](https://github.com/fluentmigrator/fluentmigrator)

## License

Free to use
