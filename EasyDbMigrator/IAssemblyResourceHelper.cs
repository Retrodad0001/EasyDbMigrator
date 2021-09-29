using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator
{
    public interface IAssemblyResourceHelper
    {
        Task<List<SqlScript>> TryConverManifestResourceStreamsToScriptsAsync(Type typeOfClassWhereScriptsAreLocated);
    }
}