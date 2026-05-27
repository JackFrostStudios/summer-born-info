namespace SummerBornInfo.Infrastructure.Persistence;

public static class PostgreSqlDatabaseBootstrapper
{
    private const string EnsurePgTrgmExtensionSql = "CREATE EXTENSION IF NOT EXISTS pg_trgm;";
    private const string ValidatePgTrgmExtensionSql = "SELECT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm');";

    public static async Task EnsureApplicationDatabaseAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
        var databaseExists = await databaseCreator.ExistsAsync(cancellationToken);

        if (!databaseExists)
        {
            await databaseCreator.CreateAsync(cancellationToken);
        }

        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            _ = await dbContext.Database.ExecuteSqlRawAsync(EnsurePgTrgmExtensionSql, cancellationToken);

            await using var command = dbContext.GetNpgsqlConnection().CreateCommand();
            command.CommandText = ValidatePgTrgmExtensionSql;

            var pgTrgmExtensionInstalled = (bool?)await command.ExecuteScalarAsync(cancellationToken);

            if (pgTrgmExtensionInstalled != true)
            {
                throw new InvalidOperationException("The PostgreSQL pg_trgm extension is required but was not installed.");
            }
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }

        _ = await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
