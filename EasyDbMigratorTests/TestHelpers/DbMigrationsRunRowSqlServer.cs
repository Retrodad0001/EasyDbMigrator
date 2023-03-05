using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.TestHelpers;

[ExcludeFromCodeCoverage] //this class is used for testing only
public record DbMigrationsRunRowSqlServer
{
    public int Id { get; }
    public DateTimeOffset Executed { get; }
    public string Filename { get; }
    public string Version { get; }

    public DbMigrationsRunRowSqlServer()
    {
    }

    public DbMigrationsRunRowSqlServer(int id, DateTimeOffset executed, string filename, string version)
    {
        Id = id;
        Executed = executed;
        Filename = filename;
        Version = version;
    }
}
