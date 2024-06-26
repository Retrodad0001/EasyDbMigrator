﻿// Ignore Spelling: Postgres
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.TestHelpers;

[ExcludeFromCodeCoverage]
public sealed record DbMigrationsRunRowPostgresServer
{
    public int Id { get; }
    public DateTime Executed { get; }
    public string Filename { get; }
    public string Version { get; }
    public DbMigrationsRunRowPostgresServer(int id, DateTime executed, string filename, string version)
    {
        Id = id;
        Executed = executed;
        Filename = filename;
        Version = version;
    }
}
