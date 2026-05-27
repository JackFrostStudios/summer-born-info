namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class PostgreSqlDatabaseBootstrapperTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture)
{
    [Fact]
    public async Task GivenFreshDatabase_WhenBootstrapped_ThenPgTrgmExtensionAndSchemaAreCreated()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var baseConnectionString = testDatabaseServerFixture.ConnectionString ?? throw new InvalidOperationException("Test database server connection string is null.");
        var connectionString = baseConnectionString.Replace("Database=postgres", $"Database={databaseName}", StringComparison.Ordinal)
            + ";Pooling=false";

        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using (var dbContext = new ApplicationDbContext(options))
            {
                await PostgreSqlDatabaseBootstrapper.EnsureApplicationDatabaseAsync(dbContext, TestContext.Current.CancellationToken);
            }

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm'), to_regclass('public.school') IS NOT NULL;";

            await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
            Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken));

            Assert.True(reader.GetBoolean(0));
            Assert.True(reader.GetBoolean(1));
        }
        finally
        {
            await DropDatabaseIfExistsAsync(testDatabaseServerFixture, databaseName);
        }
    }

    private static async Task DropDatabaseIfExistsAsync(
        IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
        string databaseName)
    {
        await using var connection = new NpgsqlConnection(testDatabaseServerFixture.ConnectionString);
        await connection.OpenAsync(CancellationToken.None);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""DROP DATABASE IF EXISTS "{databaseName}" WITH (FORCE);""";
        _ = await command.ExecuteNonQueryAsync(CancellationToken.None);
    }
}
