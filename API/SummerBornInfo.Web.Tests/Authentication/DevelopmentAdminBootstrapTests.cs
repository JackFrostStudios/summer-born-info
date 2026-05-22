namespace SummerBornInfo.Web.Tests.Authentication;

public sealed class DevelopmentAdminBootstrapTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task GivenBootstrapConfiguration_WhenApplicationStarts_ThenConfiguredAdminUserAndRoleAreCreated()
    {
        const string adminUserEmail = "bootstrap-admin@example.com";
        const string adminUserPassword = "P@ssword123!";

        await using var factory = CreateFactory(testDatabaseServerFixture, testOutputHelper, adminUserEmail, adminUserPassword);
        await factory.InitializeAsync();

        await using var scope = factory.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminRole = await roleManager.FindByNameAsync(ApplicationRoleNames.Admin);
        var adminUser = await userManager.FindByEmailAsync(adminUserEmail);

        Assert.NotNull(adminRole);
        Assert.NotNull(adminUser);
        Assert.Equal(adminUserEmail, adminUser.UserName);
        Assert.True(adminUser.EmailConfirmed);
        Assert.True(await userManager.CheckPasswordAsync(adminUser, adminUserPassword));
        Assert.True(await userManager.IsInRoleAsync(adminUser, ApplicationRoleNames.Admin));
    }

    [Fact]
    public async Task GivenExistingConfiguredUser_WhenApplicationStarts_ThenPasswordAndAdminRoleAreUpserted()
    {
        const string adminUserEmail = "bootstrap-admin@example.com";
        const string existingPassword = "OldP@ssword123!";
        const string configuredPassword = "NewP@ssword123!";

        await using var factory = new CustomWebApplicationFactory(testDatabaseServerFixture, testOutputHelper);
        await factory.InitializeAsync();
        await SeedExistingUserAsync(factory.DatabaseConnectionString, adminUserEmail, existingPassword);

        await using var scope = factory.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var bootstrapper = new DevelopmentAdminBootstrapper(
            Options.Create(new DevelopmentAdminBootstrapOptions
            {
                AdminUserEmail = adminUserEmail,
                AdminUserPassword = configuredPassword,
            }),
            roleManager,
            userManager);

        await bootstrapper.UpsertAsync(TestContext.Current.CancellationToken);

        var adminRole = await roleManager.FindByNameAsync(ApplicationRoleNames.Admin);
        var adminUser = await userManager.FindByEmailAsync(adminUserEmail);

        Assert.NotNull(adminRole);
        Assert.NotNull(adminUser);
        Assert.Equal(adminUserEmail, adminUser.UserName);
        Assert.True(adminUser.EmailConfirmed);
        Assert.False(await userManager.CheckPasswordAsync(adminUser, existingPassword));
        Assert.True(await userManager.CheckPasswordAsync(adminUser, configuredPassword));
        Assert.True(await userManager.IsInRoleAsync(adminUser, ApplicationRoleNames.Admin));
    }

    private static CustomWebApplicationFactory CreateFactory(
        IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
        ITestOutputHelper testOutputHelper,
        string adminUserEmail,
        string adminUserPassword)
    {
        return new CustomWebApplicationFactory(
            testDatabaseServerFixture,
            testOutputHelper,
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["AdminUserEmail"] = adminUserEmail,
                ["AdminUserPassword"] = adminUserPassword,
            });
    }

    private static async Task SeedExistingUserAsync(
        string databaseConnectionString,
        string email,
        string password)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(databaseConnectionString)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        _ = await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = "legacy-admin",
            NormalizedUserName = "LEGACY-ADMIN",
            EmailConfirmed = false,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
        };

        user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, password);

        _ = dbContext.Users.Add(user);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
