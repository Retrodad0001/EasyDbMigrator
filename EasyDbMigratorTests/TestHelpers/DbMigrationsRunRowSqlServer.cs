// Ignore Spelling: Sql
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

    // ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
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
