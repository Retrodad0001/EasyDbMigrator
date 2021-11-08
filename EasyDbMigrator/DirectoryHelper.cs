using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace EasyDbMigrator
{

    //when .net 6 is out checkin and support .net 6 (also build), then update visual studio to real version and install fine coverage
    
    [ExcludeFromCodeCoverage] //tested with integrationtests
    public class DirectoryHelper : IDirectoryHelper
    {
        //TODO add result when ther is test and add some test scenario's

        public async Task<List<Script>> TryGetScriptsFromDirectoryAsync(string directory)
        {
            List<Script> scripts = new List<Script>();

            DirectoryInfo directoryIno = new DirectoryInfo(directory);
            FileInfo[] files = directoryIno.GetFiles();

            foreach (FileInfo file in files)
            {
                using FileStream fileStream = new FileStream(file.FullName, FileMode.Open);
                using StreamReader reader = new StreamReader(fileStream);
              
                string content = await reader.ReadToEndAsync().ConfigureAwait(false);
                Script script = new Script(filename: file.FullName, content: content);
            }

            return scripts;
        }
    }
}
