using SummerBornInfo.TestFramework;

namespace SummerBornInfo.Web.Tests;

public class CustomWebApplicationFactory(IntegrationTestDatabaseServerFixture testDatabaseServerFixture) : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IntegrationTestDatabaseInstanceFixture integrationTestDatabaseInstanceFixture = new(testDatabaseServerFixture);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext with TestContainers connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(integrationTestDatabaseInstanceFixture.DatabaseConnectionString);
            });
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