namespace SummerBornInfo.TestFramework;

public abstract class IntegrationTestBase(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected readonly IntegrationTestDatabaseInstanceFixture integrationTestDatabaseInstanceFixture = new(testDatabaseServerFixture);
    protected ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
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
