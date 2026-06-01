namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

internal static class GdalRuntimeConfiguration
{
    private static int _configured;

    public static void Configure()
    {
        if (Interlocked.Exchange(ref _configured, 1) == 1)
        {
            return;
        }

        GdalBase.ConfigureAll();
        SetConfigOption("PROJ_NETWORK", "OFF");

        var projSearchPaths = GetProjSearchPaths();
        if (projSearchPaths.Length > 0)
        {
            Osr.SetPROJSearchPaths(projSearchPaths);
        }
    }

    private static void SetConfigOption(string key, string value)
    {
        Environment.SetEnvironmentVariable(key, value);
        Gdal.SetConfigOption(key, value);
    }

    private static string[] GetProjSearchPaths()
    {
        var searchPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        const string? missingConfigValue = null;
        AddSearchPaths(searchPaths, Gdal.GetConfigOption("PROJ_LIB", missingConfigValue));
        AddSearchPaths(searchPaths, Environment.GetEnvironmentVariable("PROJ_LIB"));
        AddSearchPaths(searchPaths, GetBundledProjDataPath());
        AddSearchPaths(searchPaths, Path.Combine(AppContext.BaseDirectory, "gdal", "share"));

        return [.. searchPaths];
    }

    private static void AddSearchPaths(HashSet<string> searchPaths, string? configuredValue)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return;
        }

        foreach (var path in configuredValue.Split(Path.PathSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (Directory.Exists(path))
            {
                searchPaths.Add(path);
            }
        }
    }

    private static string GetBundledProjDataPath()
    {
        var runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;
        if (!string.IsNullOrWhiteSpace(runtimeIdentifier))
        {
            var runtimePath = Path.Combine(
                AppContext.BaseDirectory,
                "runtimes",
                runtimeIdentifier,
                "native",
                "maxrev.gdal.core.libshared");

            if (Directory.Exists(runtimePath))
            {
                return runtimePath;
            }
        }

        return string.Empty;
    }
}
