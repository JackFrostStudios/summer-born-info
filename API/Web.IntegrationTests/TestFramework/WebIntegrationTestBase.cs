namespace SummerBornInfo.Web.Tests.TestFramework;
public class WebIntegrationTestBase(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper): IAsyncLifetime
{
    protected CustomWebApplicationFactory factory = new(testDatabaseServerFixture, testOutputHelper);

    public async ValueTask DisposeAsync()
    {
        await factory.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await factory.InitializeAsync();
    }
}
