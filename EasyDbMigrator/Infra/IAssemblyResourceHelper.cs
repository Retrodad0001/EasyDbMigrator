using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator.Infra
{
    public interface IAssemblyResourceHelper
    {
        Task<List<Script>> TryConverManifestResourceStreamsToScriptsAsync(Type customclass);
    }
}