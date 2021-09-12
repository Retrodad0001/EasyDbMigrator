using System;
using System.Reflection;

namespace EasyDbMigrator
{
    public class ProjectResourceHelper
    {
        public string[] TryGetListOfResourceNamesFromAssemblyByType(Type customClass)
        {
            Assembly? assembly = Assembly.GetAssembly(customClass);

            if (assembly == null)
            {
                return null;//TODO return more specific error
            }

            string[] resourcenames = assembly.GetManifestResourceNames(); //TODO add test when there other resources than .sql files
            return resourcenames;
        }

    }
}