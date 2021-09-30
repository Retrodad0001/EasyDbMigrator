﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests
{
    [ExcludeFromCodeCoverage] //this class is used for testing only
    public record DbMigrationsRunRow
    {
        public int Id { get; }
        public DateTimeOffset Executed { get; }
        public string Filename { get; }
        public string Version { get; }

        public DbMigrationsRunRow()
        {

        }


        public DbMigrationsRunRow(int id, DateTimeOffset executed, string filename, string version)
        {
            Id = id;
            Executed = executed;
            Filename = filename;
            Version = version;
        }
    }
}
