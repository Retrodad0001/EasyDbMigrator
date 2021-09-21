using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDbMigrator.Infra
{
    public interface IScriptsHelper
    {
        Task<List<Script>> TryConvertoScriptsInCorrectSequenceByTypeAsync(Type customclass);
    }
}