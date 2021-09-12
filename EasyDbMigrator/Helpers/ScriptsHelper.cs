using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDbMigrator.Helpers
{
    public class ScriptsHelper //add extra unittest to test this stuff, and give back result when failes (result class?)
    {
        public async Task<List<Script>> TryConvertoScriptsInCorrectSequenceByTypeAsync(Type customclass)  
        {
            Assembly? assembly = Assembly.GetAssembly(customclass);

            if (assembly is null)
                return new List<Script>();//TODO do better than this

            string[] resourcenames = new ProjectResourceHelper().TryGetListOfResourceNamesFromAssemblyByType(customclass);

            List<Script> scripts = new List<Script>();
            foreach (string resourcename in resourcenames)
            {
                using (Stream? stream = assembly.GetManifestResourceStream(resourcename))
                {
                    if (stream is null)
                    {
                        return new List<Script>();//TODO more specific result, not covering your ass kind of code here
                    }

                    using (StreamReader reader = new(stream))
                    {
                        string sqlScriptContent = await reader.ReadToEndAsync();
                        Script newSript = new Script(scriptname:resourcename, content: sqlScriptContent);
                        scripts.Add(newSript);
                    }
                }
            }

            return SetScriptsInCorrectSequence(scripts);
        }

        private List<Script> SetScriptsInCorrectSequence(List<Script> scripts)
        {
            return scripts.OrderBy(s => s.DatePartOfName)
                .ThenBy(s => s.SequenceNumberPart).ToList();
        }

    }
}