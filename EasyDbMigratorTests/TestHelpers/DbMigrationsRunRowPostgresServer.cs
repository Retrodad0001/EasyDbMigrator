using System;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable NotAccessedField.Local

namespace EasyDbMigratorTests.TestHelpers
{
    [ExcludeFromCodeCoverage]
    public record DbMigrationsRunRowPostgresServer
    {
        private readonly int _id;
        private readonly DateTime _executed;
        private readonly string _filename;
        private readonly string _version;

        public DbMigrationsRunRowPostgresServer(int id, DateTime executed, string filename, string version)
        {
            _id = id;
            _executed = executed;
            _filename = filename;
            _version = version;
        }
    }
}
