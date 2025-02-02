namespace SummerBornInfo.Web.Test.IntegrationTest;
public class PostgresTestFixture : AppFixture<Program>
{
    public static SeededData SeededData { get; private set; } = null!;
    private PostgreSqlContainer _postgreSqlContainer = null!;

    protected override async Task PreSetupAsync()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.0-alpine")
        .WithDatabase("db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

        await _postgreSqlContainer.StartAsync();
        var options = new DbContextOptionsBuilder<SchoolContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;
        await using var dbContext = new SchoolContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        SeededData = new SeededData(dbContext);
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<SchoolContext>));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddDbContext<SchoolContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });
        }
        );
    }
}
