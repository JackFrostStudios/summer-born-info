namespace SummerBornInfo.TestFramework;

public sealed class IntegrationTestDatabaseServerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public IntegrationTestDatabaseServerFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder("ghcr.io/pgmq/pg18-pgmq:v1.10.0")
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
        await _postgreSqlContainer.StartAsync(TestContext.Current.CancellationToken);
        ConnectionString = _postgreSqlContainer.GetConnectionString();

        var templateDatabaseConnectionString = ConnectionString.Replace("Database=postgres", $"Database={TemplateDataBaseName}", StringComparison.Ordinal);
        templateDatabaseConnectionString += ";Pooling=false";

        ApplicationDbContext db = new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(templateDatabaseConnectionString).Options);
        _ = await db.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        _ = await db.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        NpgmqClient npgmq = new(connectionString: db.Database.GetConnectionString() ?? throw new InvalidOperationException("Db Connection string is null"));
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
