namespace SummerBornInfo.TestFramework;

public sealed class IntegrationTestDatabaseInstanceFixture(IntegrationTestDatabaseServerFixture databaseServerFixture) : IAsyncLifetime
{
    public string DatabaseName { get; } = Guid.NewGuid().ToString();
    public string DatabaseConnectionString { get; private set; } = "";

    public async ValueTask InitializeAsync()
    {
        await using NpgsqlConnection conn = new(databaseServerFixture.ConnectionString);
        var command = conn.CreateCommand();
        command.CommandText = $"""CREATE DATABASE "{DatabaseName}" TEMPLATE "{databaseServerFixture.TemplateDataBaseName}";""";
        await conn.OpenAsync(TestContext.Current.CancellationToken);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        await conn.CloseAsync();
        DatabaseConnectionString = databaseServerFixture.ConnectionString?.Replace("Database=postgres", $"Database={DatabaseName}", StringComparison.Ordinal) ?? throw new InvalidOperationException("Database Server Fixture Connection String is null");
    }

    public async ValueTask DisposeAsync()
    {
        await using NpgsqlConnection conn = new(databaseServerFixture.ConnectionString);
        var command = conn.CreateCommand();
        command.CommandText = $"""DROP DATABASE "{DatabaseName}" WITH (FORCE);""";
        await conn.OpenAsync(CancellationToken.None);
        await command.ExecuteNonQueryAsync(CancellationToken.None);
        await conn.CloseAsync();
    }
}
