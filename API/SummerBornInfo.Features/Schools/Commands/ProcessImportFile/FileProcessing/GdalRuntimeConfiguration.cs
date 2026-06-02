namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

internal static class GdalRuntimeConfiguration
{
    private const string ProjDatabaseFileName = "proj.db";
    private const string Ostn15GridFileName = "uk_os_OSTN15_NTv2_OSGBtoETRS.tif";
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
        var searchPaths = new List<string>();
        const string? missingConfigValue = null;
        AddSearchPaths(searchPaths, Gdal.GetConfigOption("PROJ_LIB", missingConfigValue));
        AddSearchPaths(searchPaths, Environment.GetEnvironmentVariable("PROJ_LIB"));
        AddSearchPaths(searchPaths, GetBundledProjSearchPaths(AppContext.BaseDirectory, RuntimeInformation.RuntimeIdentifier));

        return [.. searchPaths];
    }

    internal static string[] GetBundledProjSearchPaths(string baseDirectory, string? runtimeIdentifier)
    {
        var searchPaths = new List<string>();
        AddBundledSearchPath(searchPaths, GetBundledRuntimePath(baseDirectory, runtimeIdentifier));
        AddBundledSearchPath(searchPaths, baseDirectory);
        AddBundledSearchPath(searchPaths, Path.Combine(baseDirectory, "gdal", "share"));

        return [.. searchPaths];
    }

    private static void AddSearchPaths(List<string> searchPaths, string? configuredValue)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return;
        }

        foreach (var path in configuredValue.Split(Path.PathSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (Directory.Exists(path))
            {
                AddSearchPath(searchPaths, path);
            }
        }
    }

    private static void AddSearchPaths(List<string> searchPaths, IEnumerable<string> configuredPaths)
    {
        foreach (var path in configuredPaths)
        {
            AddSearchPath(searchPaths, path);
        }
    }

    private static void AddBundledSearchPath(List<string> searchPaths, string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return;
        }

        var containsBundledProjData =
            File.Exists(Path.Combine(path, ProjDatabaseFileName)) ||
            File.Exists(Path.Combine(path, Ostn15GridFileName));

        if (!containsBundledProjData)
        {
            return;
        }

        AddSearchPath(searchPaths, path);
    }

    private static void AddSearchPath(List<string> searchPaths, string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (searchPaths.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        searchPaths.Add(fullPath);
    }

    private static string? GetBundledRuntimePath(string baseDirectory, string? runtimeIdentifier)
    {
        if (!string.IsNullOrWhiteSpace(runtimeIdentifier))
        {
            return Path.Combine(
                baseDirectory,
                "runtimes",
                runtimeIdentifier,
                "native",
                "maxrev.gdal.core.libshared");
        }

        return null;
    }
}
