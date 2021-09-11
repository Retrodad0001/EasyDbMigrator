using Xunit;

namespace EasyDbMigratorTests
{
    public class MigrationTests //cannot run these test parallel because these are integration test
    {
        [Fact]
        [Trait("Category", "Integrationtest")] 
        public void Can_migrate_create_database_and_run_all_migrations() 
        {

        }
    }
}
