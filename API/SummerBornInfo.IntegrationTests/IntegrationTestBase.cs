using Docker.DotNet.Models;

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
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await integrationTestDatabaseInstanceFixture.DisposeAsync();
        }
    }
}
