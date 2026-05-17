namespace SummerBornInfo.Web.Tests.TestFramework;

public class WebIntegrationTestBase(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory { get; } = new(testDatabaseServerFixture, testOutputHelper);

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await Factory.DisposeAsync();
        }
    }

    public async ValueTask InitializeAsync()
    {
        await Factory.InitializeAsync();
    }
}
