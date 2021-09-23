using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDbMigrator.Infra
{
    [ExcludeFromCodeCoverage] //is tested with integrationtest
    public class AssemblyResourceHelper : IAssemblyResourceHelper
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

        public async Task<List<Script>> TryConverManifestResourceStreamsToScriptsAsync(Type customclass)
        {
            Assembly? assembly = Assembly.GetAssembly(customclass);

            if (assembly is null)
                throw new InvalidOperationException($"assembly is null for custom-class : {customclass}");

            string[] filenames = TryGetListOfResourceNamesFromAssemblyByType(customclass);

            List<Script> scripts = new List<Script>();
            foreach (string filename in filenames)
            {
                using (Stream? stream = assembly.GetManifestResourceStream(filename))
                {
                    if (stream is null)
                        throw new InvalidOperationException($"steam cannot be null for resource name: {filename}");

                    using (StreamReader reader = new(stream))
                    {
                        string filenameWithNoNamespaces = RemoveTheNamespaceFromName(filename);

                        string sqlScriptContent = await reader.ReadToEndAsync();
                        Script newSript = new Script(filename: filenameWithNoNamespaces, content: sqlScriptContent);
                        scripts.Add(newSript);
                    }
                }
            }

            return scripts;
        }

        private string RemoveTheNamespaceFromName(string filename)
        {
            string[] split = filename.Split(".");
            string filenameWithNoNamespaces = split[^2] + "." + split[^1];
            return filenameWithNoNamespaces;
        }
    }
}