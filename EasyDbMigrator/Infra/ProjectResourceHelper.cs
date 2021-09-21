using System;
using System.Reflection;

namespace EasyDbMigrator.Helpers
{
    public class ProjectResourceHelper
    {
        public string[] TryGetListOfResourceNamesFromAssemblyByType(Type customClass)
        {
            Assembly? assembly = Assembly.GetAssembly(customClass);

            if (assembly == null)
            {
                throw new InvalidOperationException($"assembly is null for custom-class: {customClass}");
            }

            string[] resourcenames = assembly.GetManifestResourceNames();
            return resourcenames;
        }
    }
}