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
        _ = builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            var testConfigurationValues = new Dictionary<string, string?>(configurationValues, StringComparer.Ordinal)
            {
                ["ConnectionStrings:SummerbornInfo"] = integrationTestDatabaseInstanceFixture.DatabaseConnectionString,
            };

            _ = configurationBuilder.AddInMemoryCollection(testConfigurationValues);
        });

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
                _ = optionsBuilder.UseNpgsql(
                    integrationTestDatabaseInstanceFixture.DatabaseConnectionString,
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite());
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
        Environment.SetEnvironmentVariable("ConnectionStrings__SummerbornInfo", integrationTestDatabaseInstanceFixture.DatabaseConnectionString);
    }
    public override async ValueTask DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__SummerbornInfo", value: null);
        await integrationTestDatabaseInstanceFixture.DisposeAsync();
        await base.DisposeAsync();
    }
}
