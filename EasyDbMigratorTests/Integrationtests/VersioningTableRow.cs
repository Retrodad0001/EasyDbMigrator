using System;

namespace EasyDbMigratorTests.Integrationtests
{
    public record VersioningTableRow
    {
        public int Id { get; set; }
        public DateTime Executed { get; set; }
        public string ScriptName { get; set; }
        public string ScriptContent { get; set; }
        public string Version { get; set; }
    }
}
