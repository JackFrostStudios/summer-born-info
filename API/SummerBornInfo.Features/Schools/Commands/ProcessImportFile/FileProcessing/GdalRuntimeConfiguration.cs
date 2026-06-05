namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

internal static class GdalRuntimeConfiguration
{
    private const string GridShiftsDirectoryName = "GridShifts";
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
        AddSearchPaths(searchPaths, Osr.GetPROJSearchPaths());

        const string? missingConfigValue = null;
        AddSearchPaths(searchPaths, Gdal.GetConfigOption("PROJ_LIB", missingConfigValue));
        AddSearchPaths(searchPaths, Environment.GetEnvironmentVariable("PROJ_LIB"));

        var gridShiftPath = GetGridShiftSearchPath(AppContext.BaseDirectory);
        if (gridShiftPath is not null)
        {
            AddSearchPath(searchPaths, gridShiftPath);
        }

        return [.. searchPaths];
    }

    internal static string? GetGridShiftSearchPath(string baseDirectory)
    {
        var gridShiftPath = Path.Combine(baseDirectory, GridShiftsDirectoryName);
        return File.Exists(Path.Combine(gridShiftPath, Ostn15GridFileName))
            ? Path.GetFullPath(gridShiftPath)
            : null;
    }

    private static void AddSearchPath(List<string> searchPaths, string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return;
        }

        var fullPath = Path.GetFullPath(path);
        if (searchPaths.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        searchPaths.Add(fullPath);
    }

    private static void AddSearchPaths(List<string> searchPaths, string? configuredValue)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return;
        }

        foreach (var path in configuredValue.Split(Path.PathSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            AddSearchPath(searchPaths, path);
        }
    }

    private static void AddSearchPaths(List<string> searchPaths, IEnumerable<string> configuredPaths)
    {
        foreach (var path in configuredPaths)
        {
            AddSearchPath(searchPaths, path);
        }
    }
}
