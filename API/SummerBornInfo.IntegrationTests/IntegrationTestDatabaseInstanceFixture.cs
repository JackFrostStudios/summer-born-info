namespace SummerBornInfo.TestFramework;

public sealed class IntegrationTestDatabaseInstanceFixture(IntegrationTestDatabaseServerFixture databaseServerFixture) : IAsyncLifetime
{
    public readonly string DatabaseName = Guid.NewGuid().ToString();
    public string DatabaseConnectionString = "";

    public async ValueTask InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(databaseServerFixture.ConnectionString);
        var command = conn.CreateCommand();
        command.CommandText = $"""CREATE DATABASE "{DatabaseName}" TEMPLATE "{databaseServerFixture.TemplateDataBaseName}";""";
        await conn.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await conn.CloseAsync();
        DatabaseConnectionString = databaseServerFixture.ConnectionString?.Replace("Database=postgres", $"Database={DatabaseName}") ?? throw new InvalidOperationException("Database Server Fixture Connection String is null");
    }

    public async ValueTask DisposeAsync()
    {
        await using var conn = new NpgsqlConnection(databaseServerFixture.ConnectionString);
        var command = conn.CreateCommand();
        command.CommandText = $"""DROP DATABASE "{DatabaseName}" WITH (FORCE);""";
        await conn.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await conn.CloseAsync();
    }
}
