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
        await _postgreSqlContainer.StartAsync();
        ConnectionString = _postgreSqlContainer.GetConnectionString();

        var templateDatabaseConnectionString = ConnectionString.Replace("Database=postgres", $"Database={TemplateDataBaseName}");
        templateDatabaseConnectionString += ";Pooling=false";

        ApplicationDbContext db = new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(templateDatabaseConnectionString).Options);
        await db.Database.EnsureCreatedAsync();

        await db.Database.EnsureCreatedAsync();
        NpgmqClient npgmq = new(connectionString: db.Database.GetConnectionString() ?? throw new InvalidOperationException("Db Connection string is null"));
        await npgmq.InitAsync();
        await npgmq.CreateQueueAsync(EventQueue.SchoolBulkImport.Name);
        await npgmq.CreateQueueAsync(TestEventQueue.TestQueue.Name);

        await using NpgsqlConnection conn = new(ConnectionString);
        var command = conn.CreateCommand();
        command.CommandText = $"""ALTER DATABASE "{TemplateDataBaseName}" is_template=true;""";
        await conn.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await conn.CloseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}
