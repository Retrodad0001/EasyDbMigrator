using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDbMigrator.Helpers
{
    public class ScriptsHelper
    {
        public async Task<List<Script>> TryConvertoScriptsInCorrectSequenceByTypeAsync(Type customclass)  
        {
            Assembly? assembly = Assembly.GetAssembly(customclass);

            if (assembly is null)
                throw new InvalidOperationException($"assembly is null for custom-class : {customclass}");

            string[] resourcenames = new ProjectResourceHelper().TryGetListOfResourceNamesFromAssemblyByType(customclass);

            //TODO test when there are no scripts and report back something useful but not fail
            //TODO test if there are other files then .sql and report back something useful

            List<Script> scripts = new List<Script>();
            foreach (string resourcename in resourcenames)
            {
                using (Stream? stream = assembly.GetManifestResourceStream(resourcename))
                {
                    if (stream is null)
                    {
                        throw new InvalidOperationException($"steam cannot be null for resource name: {resourcename}");
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