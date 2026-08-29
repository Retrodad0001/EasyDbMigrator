using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyDbMigrator;

/// <summary>
/// Helps to get script files embedded in an assembly
/// </summary>
public sealed class AssemblyResourceHelper : IAssemblyResourceHelper
{
    /// <summary>
    /// Gets the scripts from the assembly where the type of the class is located.
    /// </summary>
    /// <param name="typeOfClassWhereScriptsAreLocated"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<List<Script>> TryGetScriptsFromAssembly(Type typeOfClassWhereScriptsAreLocated)
    {
        ArgumentNullException.ThrowIfNull(argument: typeOfClassWhereScriptsAreLocated);

        Assembly assembly = typeOfClassWhereScriptsAreLocated.Assembly;
        string[] filenames = assembly.GetManifestResourceNames();

        List<Script> scripts = new(filenames.Length);
        foreach (string filename in filenames)
        {
            await using Stream stream = assembly.GetManifestResourceStream(filename)
                ?? throw new InvalidOperationException(new StringBuilder().Append("stream cannot be null for resource name: ").Append(filename).ToString());

            using StreamReader reader = new(stream);
            string filenameWithNoNamespaces = RemoveTheNamespaceFromName(filename);

            string sqlScriptContent = await reader.ReadToEndAsync().ConfigureAwait(false);
            Script newScript = new(filenameWithNoNamespaces, sqlScriptContent);
            scripts.Add(newScript);
        }

        return scripts;
    }

    private static string RemoveTheNamespaceFromName(string filename)
    {
        string[] split = filename.Split(".");
        string filenameWithNoNamespaces =
            new StringBuilder().Append(split[^2]).Append('.').Append(split[^1]).ToString();
        return filenameWithNoNamespaces;
    }
}