namespace SummerBornInfo.Infrastructure.Persistence;

public static class PostgreSqlDatabaseBootstrapper
{
    private static readonly PostgreSqlExtension[] RequiredExtensions =
    [
        new("pg_trgm", "The PostgreSQL pg_trgm extension is required but was not installed."),
        new("postgis", "The PostgreSQL PostGIS extension is required but was not installed."),
    ];

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
            foreach (var extension in RequiredExtensions)
            {
                await using var command = dbContext.GetNpgsqlConnection().CreateCommand();
                command.CommandText = $"""CREATE EXTENSION IF NOT EXISTS "{extension.Name}";""";
                _ = await command.ExecuteNonQueryAsync(cancellationToken);

                command.CommandText = $"""SELECT EXISTS (SELECT 1 FROM pg_extension WHERE extname = '{extension.Name}');""";

                var extensionInstalled = (bool?)await command.ExecuteScalarAsync(cancellationToken);

                if (extensionInstalled != true)
                {
                    throw new InvalidOperationException(extension.ValidationFailureMessage);
                }
            }
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }

        _ = await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    private sealed record PostgreSqlExtension(string Name, string ValidationFailureMessage);
}
