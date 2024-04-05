using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator;

/// <summary>
/// Interface for getting scripts from an assembly
/// </summary>
public interface IAssemblyResourceHelper
{
    /// <summary>
    /// Tries to get scripts from an assembly
    /// </summary>
    /// <param name="typeOfClassWhereScriptsAreLocated"></param>
    /// <returns></returns>
    Task<List<Script>> TryGetScriptsFromAssembly(Type typeOfClassWhereScriptsAreLocated);
}