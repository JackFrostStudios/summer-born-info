namespace SummerBornInfo.Web.Tests.API.Schools;

public abstract class SchoolApiIntegrationTestBase : IAsyncLifetime
{
    private readonly Dictionary<string, string?> configurationValues = new(StringComparer.Ordinal);

    protected SchoolApiIntegrationTestBase(
        IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
        ITestOutputHelper testOutputHelper)
    {
        Factory = new CustomWebApplicationFactory(
            testDatabaseServerFixture,
            testOutputHelper,
            configurationValues);
    }

    protected CustomWebApplicationFactory Factory { get; }

    public async ValueTask InitializeAsync()
    {
        await Factory.InitializeAsync();
        configurationValues["ConnectionStrings:SummerbornInfo"] = Factory.DatabaseConnectionString;
        Environment.SetEnvironmentVariable("ConnectionStrings__SummerbornInfo", Factory.DatabaseConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__SummerbornInfo", value: null);
        GC.SuppressFinalize(this);
    }
}
