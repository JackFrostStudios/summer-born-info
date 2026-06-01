namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed class BritishNationalGridLocationConverterTests
{
    [Fact]
    public void GivenConverterRuntime_WhenUsed_ThenBundledProjRuntimeDataIsPresentLocally()
    {
        var runtimePath = Path.Combine(
            AppContext.BaseDirectory,
            "runtimes",
            RuntimeInformation.RuntimeIdentifier,
            "native",
            "maxrev.gdal.core.libshared");
        var gridPath = Path.Combine(runtimePath, "uk_os_OSTN15_NTv2_OSGBtoETRS.tif");
        var projDatabasePath = Path.Combine(runtimePath, "proj.db");

        Assert.True(Directory.Exists(runtimePath), $"Expected bundled GDAL runtime directory at '{runtimePath}'.");
        Assert.True(File.Exists(gridPath), $"Expected bundled OSTN15 grid at '{gridPath}'.");
        Assert.True(File.Exists(projDatabasePath), $"Expected bundled PROJ database at '{projDatabasePath}'.");
    }

    [Fact]
    public void GivenConverterRuntime_WhenConfigured_ThenProjUsesBundledOfflineRuntimeData()
    {
        var expectedRuntimePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "runtimes",
            RuntimeInformation.RuntimeIdentifier,
            "native",
            "maxrev.gdal.core.libshared"));

        Assert.True(Directory.Exists(expectedRuntimePath), $"Expected bundled GDAL runtime directory at '{expectedRuntimePath}'.");

        GdalRuntimeConfiguration.Configure();

        Assert.False(Osr.GetPROJEnableNetwork(), "Expected PROJ network access to remain disabled.");
        Assert.Contains(
            Osr.GetPROJSearchPaths().Select(Path.GetFullPath),
            path => string.Equals(path, expectedRuntimePath, StringComparison.OrdinalIgnoreCase));
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
}
