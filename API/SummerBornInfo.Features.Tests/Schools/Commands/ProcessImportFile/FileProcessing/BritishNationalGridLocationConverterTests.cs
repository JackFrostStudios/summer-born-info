namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed class BritishNationalGridLocationConverterTests
{
    [Fact]
    public void GivenConverterRuntime_WhenUsed_ThenOstn15GridIsBundledLocally()
    {
        var gridPath = Path.Combine(AppContext.BaseDirectory, "gdal", "share", "uk_os_OSTN15_NTv2_OSGBtoETRS.tif");

        Assert.True(File.Exists(gridPath), $"Expected bundled OSTN15 grid at '{gridPath}'.");
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
