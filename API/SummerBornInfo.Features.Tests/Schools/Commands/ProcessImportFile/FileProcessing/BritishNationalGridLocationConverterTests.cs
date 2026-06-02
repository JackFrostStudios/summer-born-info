namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed class BritishNationalGridLocationConverterTests
{
    private const string ProjDatabaseFileName = "proj.db";
    private const string Ostn15GridFileName = "uk_os_OSTN15_NTv2_OSGBtoETRS.tif";

    [Fact]
    public void GivenConverterRuntime_WhenInspectingBundledProjSearchPaths_ThenBundledProjDataIsPresentLocally()
    {
        var bundledSearchPaths = GdalRuntimeConfiguration.GetBundledProjSearchPaths(
            AppContext.BaseDirectory,
            RuntimeInformation.RuntimeIdentifier);
        var appBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

        Assert.NotEmpty(bundledSearchPaths);
        Assert.All(
            bundledSearchPaths,
            path => Assert.StartsWith(appBaseDirectory, Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            bundledSearchPaths,
            path => File.Exists(Path.Combine(path, Ostn15GridFileName)));
        Assert.Contains(
            bundledSearchPaths,
            path => File.Exists(Path.Combine(path, ProjDatabaseFileName)));
    }

    [Fact]
    public void GivenConverterRuntime_WhenConfigured_ThenProjUsesBundledOfflineRuntimeData()
    {
        GdalRuntimeConfiguration.Configure();

        var configuredSearchPaths = Osr.GetPROJSearchPaths()
            .Select(Path.GetFullPath)
            .ToArray();
        var bundledSearchPaths = GdalRuntimeConfiguration.GetBundledProjSearchPaths(
            AppContext.BaseDirectory,
            RuntimeInformation.RuntimeIdentifier);

        Assert.False(Osr.GetPROJEnableNetwork(), "Expected PROJ network access to remain disabled.");
        Assert.Contains(
            bundledSearchPaths,
            path => configuredSearchPaths.Contains(Path.GetFullPath(path), StringComparer.OrdinalIgnoreCase));
        Assert.True(
            ContainsLocalBundledFile(configuredSearchPaths, ProjDatabaseFileName),
            $"Expected configured PROJ search paths to expose a local '{ProjDatabaseFileName}'.");
        Assert.True(
            ContainsLocalBundledFile(configuredSearchPaths, Ostn15GridFileName),
            $"Expected configured PROJ search paths to expose a local '{Ostn15GridFileName}'.");
    }

    [Fact]
    public void GivenBuildOutputRuntimeLayout_WhenInspectingBundledProjSearchPaths_ThenRidRuntimeDirectoryIsIncluded()
    {
        var baseDirectory = CreateTempDirectory();

        try
        {
            var runtimePath = Path.Combine(baseDirectory, "runtimes", "linux-x64", "native", "maxrev.gdal.core.libshared");
            WriteBundledProjDataFile(runtimePath, ProjDatabaseFileName);
            WriteBundledProjDataFile(runtimePath, Ostn15GridFileName);
            WriteBundledProjDataFile(Path.Combine(baseDirectory, "gdal", "share"), ProjDatabaseFileName);

            var bundledSearchPaths = GdalRuntimeConfiguration.GetBundledProjSearchPaths(baseDirectory, "linux-x64");

            Assert.Contains(Path.GetFullPath(runtimePath), bundledSearchPaths, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(baseDirectory, recursive: true);
        }
    }

    [Fact]
    public void GivenFlattenedPublishLayout_WhenInspectingBundledProjSearchPaths_ThenPublishRootIsIncluded()
    {
        var baseDirectory = CreateTempDirectory();

        try
        {
            WriteBundledProjDataFile(baseDirectory, ProjDatabaseFileName);
            WriteBundledProjDataFile(baseDirectory, Ostn15GridFileName);

            var bundledSearchPaths = GdalRuntimeConfiguration.GetBundledProjSearchPaths(baseDirectory, "linux-x64");

            Assert.Contains(Path.GetFullPath(baseDirectory), bundledSearchPaths, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(baseDirectory, recursive: true);
        }
    }

    [Fact]
    public void GivenValidBritishNationalGridCoordinates_WhenTryConvertToWgs84Point_ThenWgs84PointIsReturned()
    {
        // Act
        var point = BritishNationalGridLocationConverter.TryConvertToWgs84Point("533523", "181201");

        // Assert
        Assert.NotNull(point);
        Assert.Equal(4326, point.SRID);
        Assert.InRange(point.X, -0.09d, -0.06d);
        Assert.InRange(point.Y, 51.50d, 51.53d);
    }

    [Theory]
    [InlineData("invalid", "181201")]
    [InlineData("533523", "invalid")]
    [InlineData("", "181201")]
    [InlineData("533523", "")]
    [InlineData("Infinity", "181201")]
    public void GivenInvalidCoordinateValues_WhenTryConvertToWgs84Point_ThenNullIsReturned(string easting, string northing)
    {
        // Act
        var point = BritishNationalGridLocationConverter.TryConvertToWgs84Point(easting, northing);

        // Assert
        Assert.Null(point);
    }

    [Theory]
    [InlineData("-1", "181201")]
    [InlineData("700001", "181201")]
    [InlineData("533523", "-1")]
    [InlineData("533523", "1300001")]
    public void GivenOutOfRangeBritishNationalGridCoordinates_WhenTryConvertToWgs84Point_ThenNullIsReturned(string easting, string northing)
    {
        // Act
        var point = BritishNationalGridLocationConverter.TryConvertToWgs84Point(easting, northing);

        // Assert
        Assert.Null(point);
    }

    private static bool ContainsLocalBundledFile(IEnumerable<string> searchPaths, string fileName)
    {
        var appBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

        return searchPaths.Any(path =>
        {
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(appBaseDirectory, StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(fullPath, fileName));
        });
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"summer-born-info-gdal-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void WriteBundledProjDataFile(string directoryPath, string fileName)
    {
        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(Path.Combine(directoryPath, fileName), "test");
    }
}
