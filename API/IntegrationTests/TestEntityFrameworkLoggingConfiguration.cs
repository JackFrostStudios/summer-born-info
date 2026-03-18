namespace SummerBornInfo.TestFramework;

public static class TestEntityFrameworkLoggingConfiguration
{
    private static readonly bool EnableDetailedEfCoreLogging = false;
    public static void AddLoggingToDbContextOptions(DbContextOptionsBuilder optionsBuilder, ITestOutputHelper testOutputHelper)
    {
        if (EnableDetailedEfCoreLogging)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        optionsBuilder
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddProvider(new XUnitLoggerProvider(testOutputHelper))));
    }
}
