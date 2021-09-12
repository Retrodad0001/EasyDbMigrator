using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EasyDbMigrator
{
    public class ScriptsHelper //add extra unittest to test this stuff, and give back result when failes (result class?)
    {
        public List<Script> TryConvertoScriptsInCorrectSequenceByType(Type customclass)  
        {
            Assembly? assembly = Assembly.GetAssembly(customclass);
            string[] resourcenames = new ProjectResourceHelper().TryGetListOfResourceNamesFromAssemblyByType(customclass);

            List<Script> scripts = new List<Script>();
            foreach (string resourcename in resourcenames)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourcename))
                {
                    if (stream is null)
                    {
                        return new List<Script>();//TODO more specific result, not covering your ass kind of code here
                    }

                    using (StreamReader reader = new(stream))//TODO check if tabel not created in master
                    {
                        string sqlScriptContent = reader.ReadToEnd(); //todo make stuff async
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