namespace SummerBornInfo.Web.Tests.TestFramework;

public class WebIntegrationTestBase(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory { get; } = new(testDatabaseServerFixture, testOutputHelper);

    protected Task<HttpClient> CreateAdminTestClientAsync(string email, string password)
    {
        return CreateAuthenticatedTestClientAsync(email, password, isAdmin: true);
    }

    protected async Task<HttpClient> CreateAuthenticatedTestClientAsync(string email, string password, bool isAdmin = false)
    {
        var userId = await SeedUserAsync(email, password, isAdmin);

        return await CreateAuthenticatedClientForUserAsync(userId);
    }

    protected async Task<HttpClient> CreateAuthenticatedClientForUserAsync(string email)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);

        return await CreateAuthenticatedClientForUserAsync(user.Id);
    }

    protected Task<Guid> SeedTestUserAsync(string email, string password, bool isAdmin = false)
    {
        return SeedUserAsync(email, password, isAdmin);
    }

    protected async Task<Guid> SeedUserAsync(string email, string password, bool isAdmin = false)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = CreateUser(email, password);
        _ = dbContext.Users.Add(user);

        if (isAdmin)
        {
            var role = await GetOrCreateAdminRoleAsync(dbContext, TestContext.Current.CancellationToken);
            _ = dbContext.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = user.Id,
                RoleId = role.Id,
            });
        }

        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user.Id;
    }

    protected async Task<string> GetApplicationCookieNameAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var cookieName = optionsMonitor.Get(IdentityConstants.ApplicationScheme).Cookie.Name;

        return cookieName ?? throw new InvalidOperationException("Application cookie name is not configured.");
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await Factory.DisposeAsync();
        }
    }

    public async ValueTask InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientForUserAsync(Guid userId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        Assert.NotNull(user);

        var principal = await signInManager.CreateUserPrincipalAsync(user);
        var cookieOptions = optionsMonitor.Get(IdentityConstants.ApplicationScheme);
        var authenticationTicket = new AuthenticationTicket(principal, IdentityConstants.ApplicationScheme);
        var cookieValue = cookieOptions.TicketDataFormat.Protect(authenticationTicket);
        var cookieName = cookieOptions.Cookie.Name;

        Assert.False(string.IsNullOrWhiteSpace(cookieValue));
        Assert.False(string.IsNullOrWhiteSpace(cookieName));

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{cookieName}={cookieValue}");
        return client;
    }

    private static async Task<ApplicationRole> GetOrCreateAdminRoleAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var normalizedAdminRoleName = ApplicationRoleNames.Admin.ToUpperInvariant();
        var role = await dbContext.Roles.SingleOrDefaultAsync(
            x => x.NormalizedName == normalizedAdminRoleName,
            cancellationToken);

        if (role is not null)
        {
            return role;
        }

        role = CreateAdminRole();
        _ = dbContext.Roles.Add(role);
        return role;
    }

    private static ApplicationUser CreateUser(string email, string password)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
        };

        user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, password);
        return user;
    }

    private static ApplicationRole CreateAdminRole()
    {
        return new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = ApplicationRoleNames.Admin,
            NormalizedName = ApplicationRoleNames.Admin.ToUpperInvariant(),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
        };
    }
}
