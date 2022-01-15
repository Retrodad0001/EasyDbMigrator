using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest
    public class AssemblyResourceHelper : IAssemblyResourceHelper
    {
        public static string[] TryGetManifestResourceNamesFromAssembly(Type typeOfClassWhereScriptsAreLocated)
        {
            Assembly? assembly = Assembly.GetAssembly(typeOfClassWhereScriptsAreLocated);

            if (assembly == null)
            {
                throw new InvalidOperationException($"assembly is null for custom-class: {typeOfClassWhereScriptsAreLocated}");
            }

            string[] resourcenames = assembly.GetManifestResourceNames();
            return resourcenames;
        }

        public async Task<List<Script>> TryGetScriptsFromAssembly(Type typeOfClassWhereScriptsAreLocated)
        {
            Assembly? assembly = Assembly.GetAssembly(typeOfClassWhereScriptsAreLocated);

            if (assembly is null)
                throw new InvalidOperationException($"assembly is null for custom-class : {typeOfClassWhereScriptsAreLocated}");

            string[] filenames = TryGetManifestResourceNamesFromAssembly(typeOfClassWhereScriptsAreLocated);

            List<Script> scripts = new();
            foreach (string filename in filenames)
            {
                using Stream? stream = assembly.GetManifestResourceStream(filename);
                if (stream is null)
                    throw new InvalidOperationException($"steam cannot be null for resource name: {filename}");

                using StreamReader reader = new(stream);
                string filenameWithNoNamespaces = RemoveTheNamespaceFromName(filename);

                string sqlScriptContent = await reader.ReadToEndAsync().ConfigureAwait(false); ;
                Script newSript = new(filename: filenameWithNoNamespaces, content: sqlScriptContent);
                scripts.Add(newSript);
            }

            return scripts;
        }

        private static string RemoveTheNamespaceFromName(string filename)
        {
            string[] split = filename.Split(".");
            string filenameWithNoNamespaces = split[^2] + "." + split[^1];
            return filenameWithNoNamespaces;
        }
    }
}