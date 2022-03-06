using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace EasyDbMigrator
{

    [ExcludeFromCodeCoverage] //tested with integrationTests
    public sealed class DirectoryHelper : IDirectoryHelper
    {
        public async Task<List<Script>> TryGetScriptsFromDirectoryAsync(string directory)
        {
            List<Script> scripts = new();

            DirectoryInfo directoryIno = new(directory);
            FileInfo[] files = directoryIno.GetFiles();

            foreach (FileInfo file in files)
            {
                using FileStream fileStream = new(file.FullName, FileMode.Open);
                using StreamReader reader = new(fileStream);

                string content = await reader.ReadToEndAsync().ConfigureAwait(false);
                Script script = new(file.FullName, content);
            }

            return scripts;
        }
    }
}