namespace SummerBornInfo.CoordinateConversion;

internal static class GdalRuntimeConfiguration
{
    private const string GridShiftsDirectoryName = "GridShifts";
    private const string Ostn15GridFileName = "uk_os_OSTN15_NTv2_OSGBtoETRS.tif";
    private static readonly Lock ConfigureLock = new();

    internal static void Configure()
    {
        if (GdalBase.IsConfigured)
        {
            return;
        }

        lock (ConfigureLock)
        {
            if (GdalBase.IsConfigured)
            {
                return;
            }

            GdalBase.ConfigureAll();
            Gdal.SetConfigOption("PROJ_NETWORK", "OFF");

            var gridShiftPath = GetGridShiftSearchPath() ?? throw new InvalidOperationException("Unable to configure grid shift");
            Proj.Configure([gridShiftPath]);
        }
    }

    private static string? GetGridShiftSearchPath()
    {
        var gridShiftPath = Path.Combine(AppContext.BaseDirectory, GridShiftsDirectoryName);
        return File.Exists(Path.Combine(gridShiftPath, Ostn15GridFileName))
            ? Path.GetFullPath(gridShiftPath)
            : null;
    }
}
