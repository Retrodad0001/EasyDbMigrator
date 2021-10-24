using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigratorTests.TestHelpers
{
    [ExcludeFromCodeCoverage] //this class is used for testing only
    public class DbMigrationsRunTest//TODO make record when .net 3.1 is out of LTS
    {
        public int Id { get; }
        public DateTime Executed { get; }
        public string Filename { get; }
        public string Version { get; }

        public DbMigrationsRunTest()
        {

        }

        public DbMigrationsRunTest(int id, DateTime executed, string filename, string version)
        {
            Id = id;
            Executed = executed;
            Filename = filename;
            Version = version;
        }

        public override bool Equals(object obj)//TODO remove me when .net 3.1 is out of LTS (see record)
        {
            //Check for null and compare run-time types.
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                DbMigrationsRunTest a = (DbMigrationsRunTest)obj;
                return Id == a.Id
                    && Executed == a.Executed
                    && Filename == a.Filename
                    && Version == a.Version;
            }
        }

        public override int GetHashCode()//TODO remove me when .net 3.1 is out of LTS (see record)
        {
            return HashCode.Combine(Id, Executed, Filename, Version);
        }
    }
}
