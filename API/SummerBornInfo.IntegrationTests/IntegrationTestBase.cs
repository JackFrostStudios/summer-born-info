namespace SummerBornInfo.TestFramework;

public abstract class IntegrationTestBase(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected IntegrationTestDatabaseInstanceFixture integrationTestDatabaseInstanceFixture { get; } = new(testDatabaseServerFixture);
    protected ApplicationDbContext CreateDbContext()
    {
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
        optionsBuilder
            .UseNpgsql(integrationTestDatabaseInstanceFixture.DatabaseConnectionString);

        TestEntityFrameworkLoggingConfiguration.AddLoggingToDbContextOptions(optionsBuilder, testOutputHelper);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
    public virtual async ValueTask InitializeAsync()
    {
        await integrationTestDatabaseInstanceFixture.InitializeAsync();
    }
    public virtual async ValueTask DisposeAsync()
    {
        await integrationTestDatabaseInstanceFixture.DisposeAsync();
    }
}
