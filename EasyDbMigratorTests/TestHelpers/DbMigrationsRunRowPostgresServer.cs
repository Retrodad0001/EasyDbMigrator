using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.TestHelpers
{
    [ExcludeFromCodeCoverage]
    public record DbMigrationsRunRowPostgresServer
    {
        private int Id { get; }
        private DateTime Executed { get; }
        private string Filename { get; }
        private string Version { get; }

        public DbMigrationsRunRowPostgresServer(int id, DateTime executed, string filename, string version)
        {
            Id = id;
            Executed = executed;
            Filename = filename;
            Version = version;
        }
    }
}
