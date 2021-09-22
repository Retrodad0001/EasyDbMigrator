using System;
using System.Reflection;

namespace EasyDbMigrator.Helpers
{
    public class ManifestResourceHelper
    {
        public string[] TryGetListOfResourceNamesFromAssemblyByType(Type customClass)
        {
            Assembly? assembly = Assembly.GetAssembly(customClass);

            if (assembly == null) //TODO test: edge case but still a case : assembly can be null with Assembly.GetAssembly(type)
            {
                throw new InvalidOperationException($"assembly is null for custom-class: {customClass}");
            }

            string[] resourcenames = assembly.GetManifestResourceNames();
            return resourcenames;
        }
    }
}