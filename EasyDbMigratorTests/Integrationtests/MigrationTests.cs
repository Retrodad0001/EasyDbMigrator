using EasyDbMigrator;
using EasyDbMigrator.Helpers;
using EasyDbMigratorTests.Integrationtests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TestLib;
using Xunit;

namespace EasyDbMigratorTests
{
    [ExcludeFromCodeCoverage]
    public class MigrationTests
    {
        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task Can_migrate_create_database_and_run_all_migrations_when_there_are_no_issuesAsync() //Initial Catalog=EasyDbMigrator;
        {
            try
            {
                const string connectionstring = @"Data Source = localhost,1433; User ID = sa; Password=stuffy666!; Connect Timeout = 30; Encrypt=False; TrustServerCertificate=False; ApplicationIntent=ReadWrite; MultiSubnetFailover=False";
                const string databaseName = "EasyDbMigrator";
                DateTime ExecutedDataTime = new DateTime(2021, 12, 31, 2, 16, 0);

                await DeleteDatabaseIfExistAsync(databaseName: databaseName, connectionString: connectionstring);
                
                SqlDataBaseInfo sqlDataBaseInfo = new SqlDataBaseInfo(connectionString: connectionstring
                    , databaseName: databaseName);

                DbMigrator migrator = new();
                _ = await migrator.TryApplyMigrationsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , customClass: typeof(SomeCustomClass)
                    , executedDateTime: ExecutedDataTime);

                List<VersioningTableRow> expectedRows = new List<VersioningTableRow>();
                expectedRows.Add(new VersioningTableRow ( id:1, executed:ExecutedDataTime, scriptname:"CreateDB", version:"1.0.0"));
                expectedRows.Add(new VersioningTableRow ( id:2, executed:ExecutedDataTime, scriptname:"Script2", version:"1.0.0"));
                expectedRows.Add(new VersioningTableRow ( id:3, executed:ExecutedDataTime, scriptname:"Script1", version:"1.0.0"));

                _ = new DbTestHelper().CheckMigrationsTable(connectionString: connectionstring
                    ,expectedRows: expectedRows
                    , testdbName: databaseName);
                
                //TODO ******* test: add logging checks
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
            _ = await new SqlDbHelper().TryExcecuteSingleScriptAsync(connectionString: connectionString
                , scriptName: "EasyDbMigrator.Integrationtest_dropDatabase"
                , sqlScriptContent: query);
        }
        
        //TODO *** add test that script cannot run double and log this
        //TODO *** add test that when update version table fails that migration script should be run at the end and should be logged
       
    }
}
