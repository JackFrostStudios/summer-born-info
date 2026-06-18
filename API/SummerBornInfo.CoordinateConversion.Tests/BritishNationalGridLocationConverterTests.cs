namespace SummerBornInfo.CoordinateConversion.Tests;

[TestCaseOrderer(typeof(BritishNationalGridLocationConverterTestCaseOrderer))]
public sealed class BritishNationalGridLocationConverterTests : IDisposable
{
    private const string ProjDatabaseFileName = "proj.db";
    private const string Ostn15GridFileName = "uk_os_OSTN15_NTv2_OSGBtoETRS.tif";
    private static readonly string AppBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);
    private readonly BritishNationalGridLocationConverter converter = new();

    [Fact]
    public void GivenConverter_WhenConversionIsAttemptedWithoutManualRuntimeBootstrap_ThenBundledOfflineRuntimeDataAndGridShiftsAreConfigured()
    {
        var point = converter.TryConvertToWgs84Point("533523", "181201");
        var gridShiftSearchPath = Path.Combine(AppBaseDirectory, "GridShifts");
        var configuredSearchPaths = GetConfiguredProjSearchPaths();

        Assert.NotNull(point);
        Assert.False(Osr.GetPROJEnableNetwork(), "Expected PROJ network access to remain disabled.");
        Assert.True(File.Exists(Path.Combine(gridShiftSearchPath, Ostn15GridFileName)));
        Assert.True(
            ContainsLocalFile(configuredSearchPaths, ProjDatabaseFileName),
            $"Expected configured PROJ search paths to expose a local '{ProjDatabaseFileName}'.");
        Assert.True(
            configuredSearchPaths.Contains(gridShiftSearchPath, StringComparer.OrdinalIgnoreCase),
            $"Expected configured PROJ search paths to include '{gridShiftSearchPath}'.");
    }

    [Fact]
    public void GivenValidBritishNationalGridCoordinates_WhenTryConvertToWgs84Point_ThenWgs84PointIsReturned()
    {
        // Act
        var point = converter.TryConvertToWgs84Point("533523", "181201");

        // Assert
        Assert.NotNull(point);
        Assert.Equal(4326, point.SRID);
        Assert.InRange(point.X, -0.09d, -0.06d);
        Assert.InRange(point.Y, 51.50d, 51.53d);
    }

    [Fact]
    public void GivenValidBritishNationalGridCoordinatesWhereGridShiftMatters_WhenTryConvertToWgs84Point_ThenWgs84PointIsReturned()
    {
        // Act
        var point = converter.TryConvertToWgs84Point("433962", "300363");

        // Assert
        Assert.NotNull(point);
        Assert.Equal(4326, point.SRID);
        Assert.InRange(point.X, -1.501d, -1.499d);
        Assert.InRange(point.Y, 52.599d, 52.601d);
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
        var point = converter.TryConvertToWgs84Point(easting, northing);

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
        var point = converter.TryConvertToWgs84Point(easting, northing);

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

    public void Dispose()
    {
        converter.Dispose();
    }
}

sealed file class BritishNationalGridLocationConverterTestCaseOrderer : Xunit.v3.ITestCaseOrderer
{
    IReadOnlyCollection<TTestCase> Xunit.v3.ITestCaseOrderer.OrderTestCases<TTestCase>(
        IReadOnlyCollection<TTestCase> testCases)
    {
        return
        [
            .. testCases
                .OrderBy(static testCase => GetPriority(GetTestMethodName(testCase)))
                .ThenBy(static testCase => GetTestMethodName(testCase), StringComparer.Ordinal),
        ];
    }

    private static int GetPriority(string testMethodName)
    {
        return string.Equals(
            testMethodName,
            nameof(BritishNationalGridLocationConverterTests.GivenConverter_WhenConversionIsAttemptedWithoutManualRuntimeBootstrap_ThenBundledOfflineRuntimeDataAndGridShiftsAreConfigured),
            StringComparison.Ordinal)
            ? 0
            : 1;
    }

    private static string GetTestMethodName<TTestCase>(TTestCase testCase)
    {
        return testCase?.GetType().GetProperty("TestMethodName")?.GetValue(testCase)?.ToString()
            ?? string.Empty;
    }
}
