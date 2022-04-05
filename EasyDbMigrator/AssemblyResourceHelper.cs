using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    [ExcludeFromCodeCoverage] //is tested with integrationTest
    public sealed class AssemblyResourceHelper : IAssemblyResourceHelper
    {
        public async Task<List<Script>> TryGetScriptsFromAssembly(Type typeOfClassWhereScriptsAreLocated)
        {
            var assembly = Assembly.GetAssembly(typeOfClassWhereScriptsAreLocated);

            if (assembly is null)
            {
                throw new InvalidOperationException($"assembly is null for custom-class : {typeOfClassWhereScriptsAreLocated}");
            }

            var filenames = TryGetManifestResourceNamesFromAssembly(typeOfClassWhereScriptsAreLocated);

            List<Script> scripts = new();
            foreach (string filename in filenames)
            {
                await using var stream = assembly.GetManifestResourceStream(filename);
                if (stream is null)
                {
                    throw new InvalidOperationException($"steam cannot be null for resource name: {filename}");
                }

                using StreamReader reader = new(stream);
                string filenameWithNoNamespaces = RemoveTheNamespaceFromName(filename);

                string sqlScriptContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                Script newScript = new(filenameWithNoNamespaces, sqlScriptContent);
                scripts.Add(newScript);
            }

            return scripts;
        }

        private static IEnumerable<string> TryGetManifestResourceNamesFromAssembly(Type typeOfClassWhereScriptsAreLocated)
        {
            var assembly = Assembly.GetAssembly(typeOfClassWhereScriptsAreLocated);

            if (assembly == null)
            {
                throw new InvalidOperationException($"assembly is null for custom-class: {typeOfClassWhereScriptsAreLocated}");
            }

            string[] resourceNames = assembly.GetManifestResourceNames();
            return resourceNames;
        }

        private static string RemoveTheNamespaceFromName(string filename)
        {
            string[] split = filename.Split(".");
            string filenameWithNoNamespaces = split[^2] + "." + split[^1];
            return filenameWithNoNamespaces;
        }
    }
}