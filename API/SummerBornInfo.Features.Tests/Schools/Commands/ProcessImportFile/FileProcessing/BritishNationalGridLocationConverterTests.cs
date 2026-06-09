namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed class BritishNationalGridLocationConverterTests
{
    private const string ProjDatabaseFileName = "proj.db";
    private const string Ostn15GridFileName = "uk_os_OSTN15_NTv2_OSGBtoETRS.tif";
    private static readonly string AppBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

    public BritishNationalGridLocationConverterTests()
    {
        GdalRuntimeConfiguration.Configure();
    }

    [Fact]
    public void GivenConverterRuntime_WhenInspectingGridShiftSearchPath_ThenStableGridShiftsFolderIsReturned()
    {
        var gridShiftSearchPath = GdalRuntimeConfiguration.GetGridShiftSearchPath();

        Assert.NotNull(gridShiftSearchPath);
        Assert.Equal(Path.Combine(AppBaseDirectory, "GridShifts"), gridShiftSearchPath);
        Assert.True(File.Exists(Path.Combine(gridShiftSearchPath, Ostn15GridFileName)));
    }

    [Fact]
    public void GivenConverterRuntime_WhenConfigured_ThenProjUsesBundledOfflineRuntimeDataAndGridShifts()
    {
        GdalRuntimeConfiguration.Configure();

        var configuredSearchPaths = GetConfiguredProjSearchPaths();
        var gridShiftSearchPath = GdalRuntimeConfiguration.GetGridShiftSearchPath();

        Assert.False(Osr.GetPROJEnableNetwork(), "Expected PROJ network access to remain disabled.");
        Assert.True(
            ContainsLocalFile(configuredSearchPaths, ProjDatabaseFileName),
            $"Expected configured PROJ search paths to expose a local '{ProjDatabaseFileName}'.");
        Assert.NotNull(gridShiftSearchPath);
        Assert.True(
            configuredSearchPaths.Contains(gridShiftSearchPath, StringComparer.OrdinalIgnoreCase),
            $"Expected configured PROJ search paths to include '{gridShiftSearchPath}'.");
    }

    [Fact]
    public void GivenConverterRuntime_WhenConfiguredMultipleTimes_ThenProjSearchPathsRemainDeduplicated()
    {
        GdalRuntimeConfiguration.Configure();
        GdalRuntimeConfiguration.Configure();

        var configuredSearchPaths = GetConfiguredProjSearchPaths();

        Assert.Equal(
            configuredSearchPaths.Length,
            configuredSearchPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
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

    private static string[] GetConfiguredProjSearchPaths()
    {
        return [.. Osr.GetPROJSearchPaths().Select(Path.GetFullPath)];
    }

    private static bool ContainsLocalFile(IEnumerable<string> searchPaths, string fileName)
    {
        return searchPaths.Any(path =>
        {
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(AppBaseDirectory, StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(fullPath, fileName));
        });
    }
}
