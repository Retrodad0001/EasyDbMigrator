using EasyDbMigrator.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDbMigrator.Infra
{
    public class ScriptsHelper : IScriptsHelper
    {
        public async Task<List<Script>> TryConvertoScriptsInCorrectSequenceByTypeAsync(Type customclass)
        {
            Assembly? assembly = Assembly.GetAssembly(customclass);

            if (assembly is null)
                throw new InvalidOperationException($"assembly is null for custom-class : {customclass}");

            string[] filenames = new ProjectResourceHelper().TryGetListOfResourceNamesFromAssemblyByType(customclass);

            //TODO HIGH: test migrations fail when no database can be setup
            //TODO HIGH: test migrations fail when no version table can be added
            //TODO HIGH: test migrations fail when no database can be found
            //TODO HIGH: test ignore non .sql files
            //TODO HIGH:test script cannot be run (bad script content)
            //TODO NICE: test log : how many scripts are found

            List<Script> scripts = new List<Script>();
            foreach (string filename in filenames)
            {
                using (Stream? stream = assembly.GetManifestResourceStream(filename))
                {
                    if (stream is null)
                    {
                        throw new InvalidOperationException($"steam cannot be null for resource name: {filename}");
                    }

                    using (StreamReader reader = new(stream))
                    {
                        string filenameWithNoNamespaces = RemoveTheNamespaceFromName(filename);

                        string sqlScriptContent = await reader.ReadToEndAsync();
                        Script newSript = new Script(filename: filenameWithNoNamespaces, content: sqlScriptContent);
                        scripts.Add(newSript);
                    }
                }
            }

            return SetScriptsInCorrectSequence(scripts);
        }

        private string RemoveTheNamespaceFromName(string filename)
        {
            string[] split = filename.Split(".");
            string filenameWithNoNamespaces = split[^2] + "." + split[^1];
            return filenameWithNoNamespaces;
        }

        private List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
        {
            return scripts.OrderBy(s => s.DatePartOfName)
                .ThenBy(s => s.SequenceNumberPart).ToList();
        }

    }
}