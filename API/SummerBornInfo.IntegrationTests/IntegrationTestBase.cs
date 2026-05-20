namespace SummerBornInfo.TestFramework;

public abstract class IntegrationTestBase(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected IntegrationTestDatabaseInstanceFixture IntegrationTestDatabaseInstanceFixture { get; } = new(testDatabaseServerFixture);
    protected ApplicationDbContext CreateDbContext()
    {
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
        _ = optionsBuilder
            .UseNpgsql(IntegrationTestDatabaseInstanceFixture.DatabaseConnectionString);

        TestEntityFrameworkLoggingConfiguration.AddLoggingToDbContextOptions(optionsBuilder, testOutputHelper);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
    public virtual async ValueTask InitializeAsync()
    {
        await IntegrationTestDatabaseInstanceFixture.InitializeAsync();
    }
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await IntegrationTestDatabaseInstanceFixture.DisposeAsync();
        }
    }
}
