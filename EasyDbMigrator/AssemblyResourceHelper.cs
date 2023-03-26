using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDbMigrator;

[ExcludeFromCodeCoverage] //is tested with integrationTest
public sealed class AssemblyResourceHelper : IAssemblyResourceHelper
{
    public async Task<List<Script>> TryGetScriptsFromAssembly(Type typeOfClassWhereScriptsAreLocated)
    {
        var assembly = Assembly.GetAssembly(type: typeOfClassWhereScriptsAreLocated);

        if (assembly is null)
        {
            throw new InvalidOperationException(message: $"assembly is null for custom-class : {typeOfClassWhereScriptsAreLocated}");
        }

        var filenames = TryGetManifestResourceNamesFromAssembly(typeOfClassWhereScriptsAreLocated: typeOfClassWhereScriptsAreLocated);

        List<Script> scripts = new();
        foreach (string filename in filenames)
        {
            await using var stream = assembly.GetManifestResourceStream(name: filename);
            if (stream is null)
            {
                throw new InvalidOperationException(message: $"steam cannot be null for resource name: {filename}");
            }

            using StreamReader reader = new(stream: stream);
            string filenameWithNoNamespaces = RemoveTheNamespaceFromName(filename: filename);

            string sqlScriptContent = await reader.ReadToEndAsync().ConfigureAwait(continueOnCapturedContext: false);
            Script newScript = new(filename: filenameWithNoNamespaces, content: sqlScriptContent);
            scripts.Add(item: newScript);
        }

        return scripts;
    }

    private static IEnumerable<string> TryGetManifestResourceNamesFromAssembly(Type typeOfClassWhereScriptsAreLocated)
    {
        var assembly = Assembly.GetAssembly(type: typeOfClassWhereScriptsAreLocated);

        if (assembly == null)
        {
            throw new InvalidOperationException(message: $"assembly is null for custom-class: {typeOfClassWhereScriptsAreLocated}");
        }

        string[] resourceNames = assembly.GetManifestResourceNames();
        return resourceNames;
    }

    private static string RemoveTheNamespaceFromName(string filename)
    {
        string[] split = filename.Split(separator: ".");
        string filenameWithNoNamespaces = split[^2] + "." + split[^1];
        return filenameWithNoNamespaces;
    }
}