using DotNet.Testcontainers.Builders;

namespace SummerBornInfo.TestFramework;

public sealed class IntegrationTestDatabaseServerFixture : IAsyncLifetime
{
    private const string PostgreSqlImageName = "summerborninfo-postgres-postgis-pgmq:task-2";
    private static readonly string PostgreSqlDockerfileDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SummerBornInfo.AppHost", "Postgres"));

    private readonly PostgreSqlContainer _postgreSqlContainer;

    public IntegrationTestDatabaseServerFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder(PostgreSqlImageName)
            .WithUsername("test")
            .WithPassword("test")
            .WithName($"integration_tests_postgresql_db_{Guid.NewGuid()}")
            .WithPortBinding(5432, assignRandomHostPort: true)
            .Build();
    }

    public string? ConnectionString { get; private set; }

    public string TemplateDataBaseName { get; } = Guid.NewGuid().ToString();

    public async ValueTask InitializeAsync()
    {
        var postgreSqlImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(dockerfileDirectory: PostgreSqlDockerfileDirectory)
            .WithDockerfile(dockerfile: "Dockerfile")
            .WithName(name: PostgreSqlImageName)
            .WithDeleteIfExists(deleteIfExists: false)
            .Build();

        await postgreSqlImage.CreateAsync(TestContext.Current.CancellationToken);

        await _postgreSqlContainer.StartAsync(TestContext.Current.CancellationToken);
        ConnectionString = _postgreSqlContainer.GetConnectionString();

        var templateDatabaseConnectionString = ConnectionString.Replace("Database=postgres", $"Database={TemplateDataBaseName}", StringComparison.Ordinal);
        templateDatabaseConnectionString += ";Pooling=false";

        await using ApplicationDbContext db = new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(templateDatabaseConnectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
                .Options);
        await PostgreSqlDatabaseBootstrapper.EnsureApplicationDatabaseAsync(db, TestContext.Current.CancellationToken);
        NpgmqClient npgmq = new(connectionString: templateDatabaseConnectionString);
        await npgmq.InitAsync(TestContext.Current.CancellationToken);
        await npgmq.CreateQueueAsync(EventQueue.SchoolBulkImport.Name, TestContext.Current.CancellationToken);
        await npgmq.CreateQueueAsync(TestEventQueue.TestQueue.Name, TestContext.Current.CancellationToken);

        await using NpgsqlConnection conn = new(ConnectionString);
        var command = conn.CreateCommand();
        command.CommandText = $"""ALTER DATABASE "{TemplateDataBaseName}" is_template=true;""";
        await conn.OpenAsync(TestContext.Current.CancellationToken);
        _ = await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        await conn.CloseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}
