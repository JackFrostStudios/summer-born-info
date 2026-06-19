namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class PostgreSqlDatabaseBootstrapperTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture)
{
    [Fact]
    public async Task GivenFreshDatabase_WhenBootstrapped_ThenRequiredExtensionsAndSchemaAreCreated()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var baseConnectionString = testDatabaseServerFixture.ConnectionString ?? throw new InvalidOperationException("Test database server connection string is null.");
        var connectionString = baseConnectionString.Replace("Database=postgres", $"Database={databaseName}", StringComparison.Ordinal)
            + ";Pooling=false";

        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
                .Options;

            await using (var dbContext = new ApplicationDbContext(options))
            {
                await PostgreSqlDatabaseBootstrapper.EnsureApplicationDatabaseAsync(dbContext, TestContext.Current.CancellationToken);
            }

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT " +
                "EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm'), " +
                "EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'postgis'), " +
                "to_regclass('public.school') IS NOT NULL, " +
                "EXISTS (SELECT 1 FROM geography_columns WHERE f_table_schema = 'public' AND f_table_name = 'school' AND lower(f_geography_column) = 'schoolgeometry' AND type = 'Point' AND srid = 4326), " +
                "COALESCE((SELECT pg_get_indexdef(indexrelid) " +
                "FROM pg_index idx " +
                "JOIN pg_class tbl ON idx.indrelid = tbl.oid " +
                "JOIN pg_namespace ns ON tbl.relnamespace = ns.oid " +
                "JOIN pg_class index_class ON idx.indexrelid = index_class.oid " +
                "WHERE ns.nspname = 'public' AND tbl.relname = 'school' AND index_class.relname = 'ix_school_school_geometry'), '');";

            await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
            Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken));

            Assert.True(reader.GetBoolean(0));
            Assert.True(reader.GetBoolean(1));
            Assert.True(reader.GetBoolean(2));
            Assert.True(reader.GetBoolean(3));
            Assert.Contains("USING gist", reader.GetString(4), StringComparison.Ordinal);
            Assert.Contains(@"""SchoolGeometry""", reader.GetString(4), StringComparison.Ordinal);
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
