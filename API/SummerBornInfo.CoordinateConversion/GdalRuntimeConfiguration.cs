namespace SummerBornInfo.CoordinateConversion;

internal static class GdalRuntimeConfiguration
{
    private const string GridShiftsDirectoryName = "GridShifts";
    private const string Ostn15GridFileName = "uk_os_OSTN15_NTv2_OSGBtoETRS.tif";
    private static readonly Lock ConfigureLock = new();
    private static bool isRuntimeConfigured;
    private static TestHooks hooks = TestHooks.CreateDefault();

    internal static void Configure()
    {
        if (isRuntimeConfigured)
        {
            return;
        }

        lock (ConfigureLock)
        {
            if (isRuntimeConfigured)
            {
                return;
            }

            if (!hooks.IsGdalConfigured())
            {
                hooks.ConfigureAll();
            }

            hooks.SetGdalConfigOption("PROJ_NETWORK", "OFF");

            var gridShiftPath = hooks.GetGridShiftSearchPath() ?? throw new InvalidOperationException("Unable to configure grid shift");
            hooks.ConfigureProj([gridShiftPath]);
            isRuntimeConfigured = true;
        }
    }

    internal static IDisposable UseTestHooks(TestHooks testHooks)
    {
        ArgumentNullException.ThrowIfNull(testHooks);

        lock (ConfigureLock)
        {
            var previousHooks = hooks;
            var previousIsRuntimeConfigured = isRuntimeConfigured;

            hooks = testHooks;
            isRuntimeConfigured = false;

            return new TestHookScope(previousHooks, previousIsRuntimeConfigured);
        }
    }

    private static string? GetGridShiftSearchPath()
    {
        var gridShiftPath = Path.Combine(AppContext.BaseDirectory, GridShiftsDirectoryName);
        return File.Exists(Path.Combine(gridShiftPath, Ostn15GridFileName))
            ? Path.GetFullPath(gridShiftPath)
            : null;
    }

    internal sealed class TestHooks
    {
        internal Func<bool> IsGdalConfigured { get; init; } = static () => GdalBase.IsConfigured;
        internal Action ConfigureAll { get; init; } = GdalBase.ConfigureAll;
        internal Action<string, string> SetGdalConfigOption { get; init; } = Gdal.SetConfigOption;
        internal Func<string?> GetGridShiftSearchPath { get; init; } = GdalRuntimeConfiguration.GetGridShiftSearchPath;
        internal Action<string[]> ConfigureProj { get; init; } = static searchPaths => Proj.Configure(searchPaths);

        internal static TestHooks CreateDefault()
        {
            return new TestHooks();
        }
    }

    private sealed class TestHookScope(TestHooks previousHooks, bool previousIsRuntimeConfigured) : IDisposable
    {
        public void Dispose()
        {
            lock (ConfigureLock)
            {
                hooks = previousHooks;
                isRuntimeConfigured = previousIsRuntimeConfigured;
            }
        }
    }
}
