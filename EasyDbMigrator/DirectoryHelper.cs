using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace EasyDbMigrator;


[ExcludeFromCodeCoverage] //tested with integrationTests
public sealed class DirectoryHelper : IDirectoryHelper
{
    public async Task<List<Script>> TryGetScriptsFromDirectoryAsync(string directory)
    {
        var scripts = new List<Script>();

        DirectoryInfo directoryIno = new(directory);
        var files = directoryIno.GetFiles();

        foreach (var file in files)
        {
            await using FileStream fileStream = new(file.FullName, FileMode.Open);
            using StreamReader reader = new(fileStream);

            string content = await reader.ReadToEndAsync().ConfigureAwait(false);
            Script script = new(file.FullName, content);
            scripts.Add(script);
        }

        return scripts;
    }
}