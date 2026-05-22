namespace SummerBornInfo.Web.Tests.API.CsaApplicationReviews;

public sealed class ModerateCsaApplicationReviewTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenUnauthenticatedCaller_WhenModerationPosted_ThenReturnsUnauthorized()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/admin/csa-application-reviews/rev_123/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedNonAdminCaller_WhenModerationPosted_ThenReturnsForbidden()
    {
        var client = await CreateAuthenticatedClientAsync(
            email: "volunteer@example.com",
            password: "P@ssword123!",
            isAdmin: false);

        var response = await client.PostAsJsonAsync(
            "/api/admin/csa-application-reviews/rev_123/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenUnsupportedDecisionPosted_ThenReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync(
            email: "admin@example.com",
            password: "P@ssword123!",
            isAdmin: true);

        var response = await client.PostAsJsonAsync(
            "/api/admin/csa-application-reviews/rev_123/moderation",
            new ModerateCsaApplicationReviewRequest("hold", "Needs follow-up."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("approve", "approved")]
    [InlineData("reject", "rejected")]
    public async Task GivenAuthenticatedAdminCaller_WhenSupportedDecisionPosted_ThenReturnsOkResponse(
        string decision,
        string expectedStatus)
    {
        var client = await CreateAuthenticatedClientAsync(
            email: "admin@example.com",
            password: "P@ssword123!",
            isAdmin: true);
        var startedAtUtc = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync(
            "/api/admin/csa-application-reviews/rev_123/moderation",
            new ModerateCsaApplicationReviewRequest(decision, "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var moderationResponse = await response.Content.ReadFromJsonAsync<ModerateCsaApplicationReviewResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(moderationResponse);
        Assert.Equal("rev_123", moderationResponse.ReviewId);
        Assert.Equal(expectedStatus, moderationResponse.Status);
        Assert.Equal("Looks valid.", moderationResponse.ModeratorNote);
        Assert.InRange(
            moderationResponse.ModeratedAtUtc,
            startedAtUtc.AddSeconds(-1),
            DateTime.UtcNow.AddSeconds(1));
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password, bool isAdmin)
    {
        var userId = await SeedUserAsync(email, password, isAdmin);

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

    private async Task<Guid> SeedUserAsync(string email, string password, bool isAdmin)
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
        return user.Id;
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

    private sealed record ModerateCsaApplicationReviewRequest(string Decision, string? ModeratorNote);
    private sealed record ModerateCsaApplicationReviewResponse(
        string ReviewId,
        string Status,
        DateTime ModeratedAtUtc,
        string? ModeratorNote);
}
