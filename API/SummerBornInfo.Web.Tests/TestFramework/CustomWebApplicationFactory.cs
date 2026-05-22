namespace SummerBornInfo.Web.Tests.TestFramework;

public sealed class CustomWebApplicationFactory(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper,
    IReadOnlyDictionary<string, string?>? configurationValues = null) : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IntegrationTestDatabaseInstanceFixture integrationTestDatabaseInstanceFixture = new(testDatabaseServerFixture);
    private readonly IReadOnlyDictionary<string, string?> configurationValues = configurationValues ?? new Dictionary<string, string?>(StringComparer.Ordinal);

    internal string DatabaseConnectionString => integrationTestDatabaseInstanceFixture.DatabaseConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (configurationValues.Count > 0)
        {
            _ = builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                _ = configurationBuilder.AddInMemoryCollection(configurationValues);
            });
        }

        _ = builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                _ = services.Remove(descriptor);
            }

            // Add DbContext with TestContainers connection string
            _ = services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
            {
                _ = optionsBuilder.UseNpgsql(integrationTestDatabaseInstanceFixture.DatabaseConnectionString);
                TestEntityFrameworkLoggingConfiguration.AddLoggingToDbContextOptions(optionsBuilder, testOutputHelper);
            });
        });

        _ = builder.ConfigureLogging(logging =>
        {
            _ = logging.AddProvider(new XUnitLoggerProvider(testOutputHelper));
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async ValueTask InitializeAsync()
    {
        await integrationTestDatabaseInstanceFixture.InitializeAsync();
    }
    public override async ValueTask DisposeAsync()
    {
        await integrationTestDatabaseInstanceFixture.DisposeAsync();
        await base.DisposeAsync();
    }
}
