using System;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable NotAccessedField.Local

namespace EasyDbMigratorTests.TestHelpers
{
    [ExcludeFromCodeCoverage]
    public record DbMigrationsRunRowPostgresServer
    {
        private int _id;
        private DateTime _executed;
        private string _filename;
        private string _version;

        public DbMigrationsRunRowPostgresServer(int id, DateTime executed, string filename, string version)
        {
            _id = id;
            _executed = executed;
            _filename = filename;
            _version = version;
        }
    }
}
