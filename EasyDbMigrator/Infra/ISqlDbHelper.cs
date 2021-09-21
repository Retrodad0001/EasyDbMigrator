using EasyDbMigrator.Helpers;
using System;
using System.Threading.Tasks;

namespace EasyDbMigrator.Infra
{
    public interface ISqlDbHelper
    {
        Task<Result<RunMigrationResult>> RunDbMigrationScriptWhenNotRunnedBeforeAsync(SqlDataBaseInfo sqlDataBaseInfo, Script script, DateTime executedDateTime);
        Task<bool> TryExcecuteSingleScriptAsync(string connectionString, string scriptName, string sqlScriptContent);
    }
}