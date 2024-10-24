using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using SummerBorn.Infrastructure.Data;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace SummerBorn.Infrastructure.Test.DatabaseSetup;
public class DatabaseConfiguration : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17.0-alpine")
        .WithDatabase("db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

    public SchoolContext SchoolContext { get; private set; } = null!;
    public SeededData SeededData { get; private set; } = null!;
    private DbConnection _connection = null!;
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<SchoolContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        SchoolContext = new SchoolContext(options);
        await SchoolContext.Database.EnsureCreatedAsync();
        _connection = SchoolContext.Database.GetDbConnection();
        await _connection.OpenAsync();

        SeededData = new SeededData(SchoolContext);
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _container.DisposeAsync();
    }
}
