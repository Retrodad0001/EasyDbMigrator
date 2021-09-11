using EasyDbMigrator;
using EasyDbMigratorTests.Integrationtests;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

//TODO add rule underscore

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace EasyDbMigratorTests
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    [ExcludeFromCodeCoverage]
    public class MigrationTests //cannot run these test parallel because these are integration test
    {
        [Fact]
        [Trait("Category", "Integrationtest")]
        public async Task Can_migrate_create_database_and_run_all_migrations_when_there_are_no_issuesAsync() //Initial Catalog=EasyDbMigrator;
        {
            const string connectionstring = @"Data Source = localhost,1433; User ID = sa; Password=stuffy666!; Connect Timeout = 30; Encrypt=False; TrustServerCertificate=False; ApplicationIntent=ReadWrite; MultiSubnetFailover=False";

            DbMigrator migrator = new(connectionstring);
            _ = await migrator.TryApplyMigrationsAsync();

            DbTestHelper helper = new DbTestHelper(connectionstring);

            const string databaseName = "EasyDbMigrator";
    //TODO        _ = helper.CheckMigrationsTable().Should().BeTrue(); ;//TODO check if these info is in migration table and if table exist
        }

        //TODO test rollback script when error in script or otherwise
        //TODO test when there are no scripts
        //TODO add retry policy
       
      
    }
}
