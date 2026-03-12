namespace SummerBornInfo.TestFramework;
public sealed class IntegrationTestDatabaseServerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public IntegrationTestDatabaseServerFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder("postgres:alpine")
            .WithUsername("test")
            .WithPassword("test")
            .WithName("integration_tests_postgresql_db")
            .WithPortBinding(5431, 5432)
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

        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(templateDatabaseConnectionString).Options);
        await db.Database.EnsureCreatedAsync();

        await using var conn = new NpgsqlConnection(ConnectionString);
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
