namespace SummerBornInfo.CoordinateConversion.Tests;

public sealed class GdalRuntimeConfigurationTests
{
    [Fact]
    public void GivenBootstrapStepFailsAfterGdalBaseConfigure_WhenConfigureIsRetried_ThenRemainingConfigurationIsRetriedAndSuccessIsCached()
    {
        var gdalConfigured = false;
        var configureAllCalls = 0;
        var setConfigOptionCalls = 0;
        var getGridShiftSearchPathCalls = 0;
        var configureProjCalls = 0;
        var expectedGridShiftPath = Path.Combine("C:", "grid-shifts");

        using var _ = GdalRuntimeConfiguration.UseTestHooks(new GdalRuntimeConfiguration.TestHooks
        {
            IsGdalConfigured = () => gdalConfigured,
            ConfigureAll = () =>
            {
                configureAllCalls++;
                gdalConfigured = true;
            },
            SetGdalConfigOption = (_, _) => setConfigOptionCalls++,
            GetGridShiftSearchPath = () =>
            {
                getGridShiftSearchPathCalls++;
                return expectedGridShiftPath;
            },
            ConfigureProj = paths =>
            {
                configureProjCalls++;

                var configuredGridShiftPath = Assert.Single(paths);
                Assert.Equal(expectedGridShiftPath, configuredGridShiftPath);

                if (configureProjCalls == 1)
                {
                    throw new InvalidOperationException("Simulated PROJ configuration failure.");
                }
            },
        });

        var firstAttempt = Assert.Throws<InvalidOperationException>(GdalRuntimeConfiguration.Configure);
        Assert.Equal("Simulated PROJ configuration failure.", firstAttempt.Message);

        GdalRuntimeConfiguration.Configure();
        GdalRuntimeConfiguration.Configure();

        Assert.Equal(1, configureAllCalls);
        Assert.Equal(2, setConfigOptionCalls);
        Assert.Equal(2, getGridShiftSearchPathCalls);
        Assert.Equal(2, configureProjCalls);
    }
}
