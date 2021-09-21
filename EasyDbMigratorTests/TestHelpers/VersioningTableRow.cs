using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests
{
    [ExcludeFromCodeCoverage] //this class is used for testing only
    public record VersioningTableRow
    {
        public int Id { get; }
        public DateTime Executed { get; }
        public string Filename { get; }
        public string Version { get; }

        public VersioningTableRow(int id, DateTime executed, string filename, string version)
        {
            Id = id;
            Executed = executed;
            Filename = filename;
            Version = version;
        }
    }
}
