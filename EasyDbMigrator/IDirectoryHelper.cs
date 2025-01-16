using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator;

/// <summary>
/// Interface for getting scripts from a directory and mocking this stuff out.
/// </summary>
public interface IDirectoryHelper
{
    /// <summary>
    /// Try to get scripts from a directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    Task<List<Script>> TryGetScriptsFromDirectoryAsync(string directory);
}
