namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextIdentityTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    private static readonly string[] ExpectedIdentityAndDomainTableNames =
    [
        "application_role",
        "application_role_claim",
        "application_user",
        "application_user_claim",
        "application_user_login",
        "application_user_role",
        "application_user_token",
        "school",
    ];

    [Fact]
    public async Task GivenApplicationDatabase_WhenInspectingTables_ThenIdentityTablesExistAlongsideExistingTables()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(IntegrationTestDatabaseInstanceFixture.DatabaseConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT tablename " +
            "FROM pg_tables " +
            "WHERE schemaname = 'public' " +
            "AND tablename = ANY (@tableNames) " +
            "ORDER BY tablename;";
        _ = command.Parameters.AddWithValue("tableNames", ExpectedIdentityAndDomainTableNames);

        List<string> tableNames = [];
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        // Act
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            tableNames.Add(reader.GetString(0));
        }

        // Assert
        Assert.Equal(ExpectedIdentityAndDomainTableNames, tableNames);
    }

    [Fact]
    public async Task GivenIdentityUserAndAdminRole_WhenSavingToDatabase_ThenUserRoleAssignmentCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = ApplicationRoleNames.Admin,
            NormalizedName = ApplicationRoleNames.Admin.ToUpperInvariant(),
        };
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            UserName = "admin@example.com",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        _ = dbContext.Roles.Add(role);
        _ = dbContext.Users.Add(user);
        _ = dbContext.Set<IdentityUserRole<Guid>>().Add(new IdentityUserRole<Guid>
        {
            UserId = user.Id,
            RoleId = role.Id,
        });

        // Act
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var verificationContext = CreateDbContext();
        var savedUser = await verificationContext.Users.SingleAsync(
            x => x.NormalizedUserName == user.NormalizedUserName,
            TestContext.Current.CancellationToken);
        var savedRole = await verificationContext.Roles.SingleAsync(
            x => x.NormalizedName == role.NormalizedName,
            TestContext.Current.CancellationToken);
        var savedUserRole = await verificationContext.Set<IdentityUserRole<Guid>>().SingleAsync(
            x => x.UserId == user.Id && x.RoleId == role.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(user.Email, savedUser.Email);
        Assert.Equal(role.Name, savedRole.Name);
        Assert.Equal(user.Id, savedUserRole.UserId);
        Assert.Equal(role.Id, savedUserRole.RoleId);
    }
}
