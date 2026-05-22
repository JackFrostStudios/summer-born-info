namespace SummerBornInfo.Web.Tests.Authentication;

public sealed class AdminAuthEndpointsTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenAdminCredentials_WhenSignInPosted_ThenResponseSetsAuthenticationCookie()
    {
        const string email = "admin@example.com";
        const string password = "P@ssword123!";
        await SeedUserAsync(email, password, isAdmin: true);
        var cookieName = await GetApplicationCookieNameAsync();

        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
        });

        var response = await client.PostAsJsonAsync(
            "/api/admin/auth/sign-in",
            new SignInRequest(email, password),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertAuthenticationCookieWasSet(response, cookieName);
    }

    [Fact]
    public async Task GivenInvalidCredentials_WhenSignInPosted_ThenReturnsUnauthorizedWithoutAuthenticationCookie()
    {
        const string email = "admin@example.com";
        await SeedUserAsync(email, "P@ssword123!", isAdmin: true);
        var cookieName = await GetApplicationCookieNameAsync();

        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/admin/auth/sign-in",
            new SignInRequest(email, "incorrect-password"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertAuthenticationCookieWasNotSet(response, cookieName);
    }

    [Fact]
    public async Task GivenNonAdminCredentials_WhenSignInPosted_ThenReturnsForbiddenWithoutAuthenticationCookie()
    {
        const string email = "volunteer@example.com";
        const string password = "P@ssword123!";
        await SeedUserAsync(email, password, isAdmin: false);
        var cookieName = await GetApplicationCookieNameAsync();

        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/admin/auth/sign-in",
            new SignInRequest(email, password),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        AssertAuthenticationCookieWasNotSet(response, cookieName);
    }

    [Fact]
    public async Task GivenSignedInAdmin_WhenSignOutPosted_ThenResponseClearsAuthenticationCookie()
    {
        const string email = "admin@example.com";
        const string password = "P@ssword123!";
        await SeedUserAsync(email, password, isAdmin: true);
        var cookieName = await GetApplicationCookieNameAsync();

        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
        });

        var signInResponse = await client.PostAsJsonAsync(
            "/api/admin/auth/sign-in",
            new SignInRequest(email, password),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, signInResponse.StatusCode);

        var signOutResponse = await client.PostAsync(
            "/api/admin/auth/sign-out",
            content: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, signOutResponse.StatusCode);

        var setCookieHeader = GetAuthenticationCookieHeader(signOutResponse, cookieName);
        Assert.Contains("expires=", setCookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    private async Task SeedUserAsync(string email, string password, bool isAdmin)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = CreateUser(email, password);
        _ = dbContext.Users.Add(user);

        if (isAdmin)
        {
            var role = CreateAdminRole();
            _ = dbContext.Roles.Add(role);
            _ = dbContext.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = user.Id,
                RoleId = role.Id,
            });
        }

        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<string> GetApplicationCookieNameAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var cookieName = optionsMonitor.Get(IdentityConstants.ApplicationScheme).Cookie.Name;

        return cookieName ?? throw new InvalidOperationException("Application cookie name is not configured.");
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

    private static void AssertAuthenticationCookieWasSet(HttpResponseMessage response, string cookieName)
    {
        var setCookieHeader = GetAuthenticationCookieHeader(response, cookieName);
        Assert.DoesNotContain("expires=", setCookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertAuthenticationCookieWasNotSet(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            return;
        }

        Assert.DoesNotContain(
            setCookieValues,
            value => value.Contains($"{cookieName}=", StringComparison.Ordinal));
    }

    private static string GetAuthenticationCookieHeader(HttpResponseMessage response, string cookieName)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieValues));

        return Assert.Single(
            setCookieValues,
            value => value.Contains($"{cookieName}=", StringComparison.Ordinal));
    }

    private sealed record SignInRequest(string Email, string Password);
}
