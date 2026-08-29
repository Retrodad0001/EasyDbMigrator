using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EasyDbMigrator;

/// <summary>
/// Helper class to get scripts from a directory
/// </summary>
public sealed class DirectoryHelper : IDirectoryHelper
{
    /// <summary>
    /// Get all scripts from a directory
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public async Task<List<Script>> TryGetScriptsFromDirectoryAsync(string directory)
    {
        var scriptsToRun = new List<Script>();

        DirectoryInfo directoryIno = new(directory);
        var files = directoryIno.GetFiles();

        foreach (FileInfo? file in files)
        {
            await using FileStream fileStream = new(file.FullName, FileMode.Open);
            using StreamReader reader = new(fileStream);

            string content = await reader.ReadToEndAsync().ConfigureAwait(false);
            Script script = new(file.Name, content);
            scriptsToRun.Add(script);
        }

        return scriptsToRun;
    }
}
