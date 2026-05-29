namespace SummerBornInfo.Web.Tests.API.Schools;

public abstract class SchoolApiIntegrationTestBase(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory { get; } = new(testDatabaseServerFixture, testOutputHelper);

    public async ValueTask InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
