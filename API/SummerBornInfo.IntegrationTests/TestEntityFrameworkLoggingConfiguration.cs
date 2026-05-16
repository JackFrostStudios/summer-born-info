namespace SummerBornInfo.TestFramework;

public static class TestEntityFrameworkLoggingConfiguration
{
    private const bool EnableDetailedEfCoreLogging = false;
    public static void AddLoggingToDbContextOptions(DbContextOptionsBuilder optionsBuilder, ITestOutputHelper testOutputHelper)
    {
        if (EnableDetailedEfCoreLogging)
        {
            // Const is set to true to enable logging if required.
            // Config file has been avoided here as it just adds complication for now as this is the only configuration value.
#pragma warning disable CS0162 // Unreachable code detected
            optionsBuilder
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
#pragma warning restore CS0162 // Unreachable code detected
        }

        optionsBuilder
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddProvider(new XUnitLoggerProvider(testOutputHelper))));
    }
}
