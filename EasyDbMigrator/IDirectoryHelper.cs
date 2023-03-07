using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator;

public interface IDirectoryHelper
{
    Task<List<Script>> TryGetScriptsFromDirectoryAsync(string directory);
}
