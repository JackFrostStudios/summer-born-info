namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

#pragma warning disable SYSLIB1054

internal static class GdalRuntimeConfiguration
{
    private const uint LoadLibrarySearchDefaultDirectories = 0x00001000;
    private static int _configured;

    public static void Configure()
    {
        if (Interlocked.Exchange(ref _configured, 1) == 1)
        {
            return;
        }

        var gdalRootPath = Path.Combine(AppContext.BaseDirectory, "gdal");
        var nativeLibraryPath = Path.Combine(gdalRootPath, Environment.Is64BitProcess ? "x64" : "x86");

        if (!Directory.Exists(nativeLibraryPath))
        {
            throw new DirectoryNotFoundException($"GDAL native directory not found at '{nativeLibraryPath}'.");
        }

        if (OperatingSystem.IsWindows())
        {
            _ = SetDefaultDllDirectories(LoadLibrarySearchDefaultDirectories);
            _ = AddDllDirectory(nativeLibraryPath);

            var pluginPath = Path.Combine(nativeLibraryPath, "plugins");
            if (Directory.Exists(pluginPath))
            {
                _ = AddDllDirectory(pluginPath);
            }
        }

        SetConfigOption("GDAL_DATA", Path.Combine(gdalRootPath, "data"));
        SetConfigOption("GEOTIFF_CSV", Path.Combine(gdalRootPath, "data"));

        var pluginDirectory = Path.Combine(nativeLibraryPath, "plugins");
        if (Directory.Exists(pluginDirectory))
        {
            SetConfigOption("GDAL_DRIVER_PATH", pluginDirectory);
        }

        var projLibraryPath = Path.Combine(gdalRootPath, "share");
        SetConfigOption("PROJ_LIB", projLibraryPath);
        Osr.SetPROJSearchPaths([projLibraryPath]);

        var certificateBundlePath = Path.Combine(gdalRootPath, "curl-ca-bundle.crt");
        if (File.Exists(certificateBundlePath))
        {
            SetConfigOption("GDAL_CURL_CA_BUNDLE", certificateBundlePath);
        }

        Gdal.AllRegister();
        Ogr.RegisterAll();
    }

    private static void SetConfigOption(string key, string value)
    {
        Environment.SetEnvironmentVariable(key, value);
        Gdal.SetConfigOption(key, value);
    }

    [DllImport("kernel32", EntryPoint = "SetDefaultDllDirectories", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDefaultDllDirectories(uint directoryFlags);

    [DllImport("kernel32", EntryPoint = "AddDllDirectory", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint AddDllDirectory(string newDirectory);
}

#pragma warning restore SYSLIB1054
