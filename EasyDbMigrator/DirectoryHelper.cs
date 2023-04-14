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
        var scriptsToRun = new List<Script>();

        DirectoryInfo directoryIno = new(path: directory);
        var files = directoryIno.GetFiles();

        foreach (var file in files)
        {
            await using FileStream fileStream = new(path: file.FullName, mode: FileMode.Open);
            using StreamReader reader = new(stream: fileStream);

            string content = await reader.ReadToEndAsync().ConfigureAwait(continueOnCapturedContext: false);
            Script script = new(filename: file.FullName, content: content);
            scriptsToRun.Add(item: script);
        }

        return scriptsToRun;
    }
}