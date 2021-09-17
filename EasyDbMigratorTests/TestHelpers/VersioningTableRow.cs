using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests
{
    [ExcludeFromCodeCoverage] //this class is used for testing only
    public record VersioningTableRow
    {
        public int Id { get; }
        public DateTime Executed { get; }
        public string Scriptname { get; }
        public string Version { get; }

        public VersioningTableRow(int id, DateTime executed, string scriptname, string version)
        {
            if (string.IsNullOrEmpty(scriptname))
            {
                throw new ArgumentException($"'{nameof(scriptname)}' cannot be null or empty.", nameof(scriptname));
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException($"'{nameof(version)}' cannot be null or empty.", nameof(version));
            }

            Id = id;
            Executed = executed;
            Scriptname = scriptname;
            Version = version;
        }
    }
}
