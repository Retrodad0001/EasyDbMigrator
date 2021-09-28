using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

//TODO wiki: sql resources should be embedded
//TODO wiki: add example code integration test with xunit

//TODO: PRIO check in integration test: 0001-01-01 00:00:00.0000000 !!!

namespace EasyDbMigrator
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

        public async Task<List<SqlScript>> TryConverManifestResourceStreamsToScriptsAsync(Type customclass)
        {
            Assembly? assembly = Assembly.GetAssembly(customclass);

            if (assembly is null)
                throw new InvalidOperationException($"assembly is null for custom-class : {customclass}");

            string[] filenames = TryGetListOfResourceNamesFromAssemblyByType(customclass);

            List<SqlScript> scripts = new List<SqlScript>();
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
                        SqlScript newSript = new SqlScript(filename: filenameWithNoNamespaces, content: sqlScriptContent);
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