namespace SummerBornInfo.TestFramework;

public sealed class IntegrationTestDatabaseServerFixture : IAsyncLifetime
{
    private const string PostgreSqlImageRepositoryName = "summerborninfo-postgres-postgis-pgmq";
    private static readonly string PostgreSqlDockerfileDirectory = PostgresDockerfilePath.PostgreSqlDockerfileDirectory;

    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly string _postgreSqlImageName;
    private readonly string _postgreSqlImageBuildMutexName;

    public IntegrationTestDatabaseServerFixture()
    {
        var postgreSqlImageVersion = PostgreSqlDockerImageVersion.Version;
        var postgreSqlImageIdentity = CreatePostgreSqlImageIdentity(postgreSqlImageVersion);
        _postgreSqlImageName = postgreSqlImageIdentity.ImageName;
        _postgreSqlImageBuildMutexName = postgreSqlImageIdentity.BuildMutexName;

        _postgreSqlContainer = new PostgreSqlBuilder(_postgreSqlImageName)
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
        var postgreSqlImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(dockerfileDirectory: PostgreSqlDockerfileDirectory)
            .WithDockerfile(dockerfile: "Dockerfile")
            .WithName(name: _postgreSqlImageName)
            .WithDeleteIfExists(deleteIfExists: false)
            .Build();

        await CreatePostgreSqlImageAsync(
            imageBuildMutexName: _postgreSqlImageBuildMutexName,
            createImageAsync: ct => postgreSqlImage.CreateAsync(ct),
            TestContext.Current.CancellationToken);

        await _postgreSqlContainer.StartAsync(TestContext.Current.CancellationToken);
        ConnectionString = _postgreSqlContainer.GetConnectionString();

        var templateDatabaseConnectionString = ConnectionString.Replace("Database=postgres", $"Database={TemplateDataBaseName}", StringComparison.Ordinal);
        templateDatabaseConnectionString += ";Pooling=false;";

        await using ApplicationDbContext db = new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(templateDatabaseConnectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
                .Options);
        await PostgreSqlDatabaseBootstrapper.EnsureApplicationDatabaseAsync(db, TestContext.Current.CancellationToken);
        NpgmqClient npgmq = new(connectionString: templateDatabaseConnectionString);
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

    // Visual Studio can start multiple test hosts in parallel, and Testcontainers packages the shared
    // Docker build context through one temp archive path, so concurrent image creation races on that file.
    private static async Task CreatePostgreSqlImageAsync(
        string imageBuildMutexName,
        Func<CancellationToken, Task> createImageAsync,
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var mutex = new Mutex(initiallyOwned: false, imageBuildMutexName);
            var acquired = false;

            try
            {
                try
                {
                    acquired = mutex.WaitOne(TimeSpan.FromMinutes(2));
                }
                catch (AbandonedMutexException)
                {
                    acquired = true;
                }

                if (!acquired)
                {
                    throw new TimeoutException("Timed out waiting to acquire the PostgreSQL test image build mutex.");
                }

                RunImageCreationSynchronously(createImageAsync, cancellationToken);
            }
            finally
            {
                if (acquired)
                {
                    mutex.ReleaseMutex();
                }
            }
        }, cancellationToken);
    }

    internal static PostgreSqlTestImageIdentity CreatePostgreSqlImageIdentity(string postgreSqlImageVersion)
    {
        return new PostgreSqlTestImageIdentity(
            ImageName: $"{PostgreSqlImageRepositoryName}:{postgreSqlImageVersion}",
            BuildMutexName: $@"Global\{PostgreSqlImageRepositoryName}-{postgreSqlImageVersion}-build");
    }

    [SuppressMessage("Usage", "MA0042:Prefer using 'await'", Justification = "Named mutex ownership is thread-affine, so acquire, build, and release must stay on the same worker thread.")]
    private static void RunImageCreationSynchronously(Func<CancellationToken, Task> createImageAsync, CancellationToken cancellationToken)
    {
        createImageAsync(cancellationToken).GetAwaiter().GetResult();
    }

    internal readonly record struct PostgreSqlTestImageIdentity(string ImageName, string BuildMutexName);
}
