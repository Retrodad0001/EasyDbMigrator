using EasyDbMigrator;
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
    public class MigrationTests //cannot run these test parallel because these are integration test
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

                DbTestHelper helper = new DbTestHelper(connectionstring);

                await DeleteDatabaseIfExistAsync(databaseName, helper);
                
                SqlDataBaseInfo sqlDataBaseInfo = new SqlDataBaseInfo(connectionString: connectionstring
                    , databaseName: databaseName);

                DbMigrator migrator = new();
                _ = await migrator.TryApplyMigrationsAsync(sqlDataBaseInfo: sqlDataBaseInfo
                    , customClass: typeof(SomeCustomClass)
                    , executedDateTime: ExecutedDataTime);

                List<VersioningTableRow> expectedRows = new List<VersioningTableRow>();
                expectedRows.Add(new VersioningTableRow { Id = 1, Executed = ExecutedDataTime, ScriptContent = "xx", ScriptName= "CreateDB", Version = "1.0.0" });
                expectedRows.Add(new VersioningTableRow { Id = 2, Executed = ExecutedDataTime, ScriptContent = "xx", ScriptName = "Script2", Version = "1.0.0" });
                expectedRows.Add(new VersioningTableRow { Id = 3, Executed = ExecutedDataTime, ScriptContent = "xx", ScriptName = "Script1", Version = "1.0.0" });

                _ = helper.CheckMigrationsTable(expectedRows, databaseName);//TODO check if these info is in migration table and if table exist
                //TODO ******* test: add logging checks
            }
#pragma warning disable CA1031 // Do not catch general exception types, for sake of testing this is no problem
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Assert.True(false, ex.ToString());
            }
        }

        private async Task DeleteDatabaseIfExistAsync(string databaseName, DbTestHelper helper)
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
            _ = await helper.TryExecuteSQLScriptAsync(query);
        }
        
        //TODO *** add test that script cannot run double and log this
        //TODO *** add test that when update version table fails that migration script should be run at the end and should be logged
       
    }
}
