using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyDbMigrator;

/// <summary>
/// Helps to get script files embedded in an assembly
/// </summary>
[ExcludeFromCodeCoverage] //is tested with integrationTest
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
        Assembly? assembly = Assembly.GetAssembly(typeOfClassWhereScriptsAreLocated);

        string[] filenames = TryGetManifestResourceNamesFromAssembly(typeOfClassWhereScriptsAreLocated);

        List<Script> scripts = new(filenames.Length);
        foreach (string filename in filenames)
        {
            await using Stream? stream = assembly?.GetManifestResourceStream(filename);
            if (stream is null)
            {
                throw new InvalidOperationException(new StringBuilder()
                    .Append("steam cannot be null for resource name: ")
                    .Append(filename)
                    .ToString());
            }

            using StreamReader reader = new(stream);
            string filenameWithNoNamespaces = RemoveTheNamespaceFromName(filename);

            string sqlScriptContent = await reader.ReadToEndAsync().ConfigureAwait(false);
            Script newScript = new(filenameWithNoNamespaces, sqlScriptContent);
            scripts.Add(newScript);
        }

        return scripts;
    }

    private static string[] TryGetManifestResourceNamesFromAssembly(Type typeOfClassWhereScriptsAreLocated)
    {
        Assembly? assembly = Assembly.GetAssembly(typeOfClassWhereScriptsAreLocated);

        if (assembly is null)
        {
            throw new InvalidOperationException(new StringBuilder().Append("assembly is null for custom-class: ")
                .Append(typeOfClassWhereScriptsAreLocated)
                .ToString());
        }

        string[] resourceNames = assembly.GetManifestResourceNames();
        return resourceNames;
    }

    private static string RemoveTheNamespaceFromName(string filename)
    {
        string[] split = filename.Split(".");
        string filenameWithNoNamespaces =
            new StringBuilder().Append(split[^2]).Append('.').Append(split[^1]).ToString();
        return filenameWithNoNamespaces;
    }
}