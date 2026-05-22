namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class CreateSchoolsImportRequestTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenUnauthenticatedCaller_WhenSchoolImportPosted_ThenReturnsUnauthorized()
    {
        var client = Factory.CreateClient();
        using var content = CreateImportContent();

        var response = await client.PostAsync("/api/admin/school-imports", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, await GetImportRequestCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenAuthenticatedNonAdminCaller_WhenSchoolImportPosted_ThenReturnsForbidden()
    {
        var client = await CreateAuthenticatedClientAsync(
            email: "volunteer@example.com",
            password: "P@ssword123!",
            isAdmin: false);
        using var content = CreateImportContent();

        var response = await client.PostAsync("/api/admin/school-imports", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, await GetImportRequestCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenSchoolImportPosted_ThenReturnsAcceptedAndBackgroundWorkerProcessesTheFile()
    {
        var client = await CreateAuthenticatedClientAsync(
            email: "admin@example.com",
            password: "P@ssword123!",
            isAdmin: true);
        using var content = CreateImportContent();

        var response = await client.PostAsync("/api/admin/school-imports", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var importResponse = await response.Content.ReadFromJsonAsync<ImportSchoolsResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(importResponse);
        Assert.NotEqual(Guid.Empty, importResponse.ImportRequestId);
        Assert.Equal(ImportSchoolsResponse.QueuedStatus, importResponse.Status);

        var request = await WaitForImportRequestAsync(importResponse.ImportRequestId, TestContext.Current.CancellationToken);
        Assert.NotNull(request);
        Assert.Equal(2, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, request.Status);
        Assert.Empty(request.Failures);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schools = await dbContext.Schools.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, schools.Count);
    }

    private static StreamContent CreateImportContent()
    {
        StreamContent content = new(ExampleImportFile.GetExampleImportFileContent());
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        return content;
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

    private async Task<int> GetImportRequestCountAsync(CancellationToken cancellationToken)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.SchoolBulkImportRequests.CountAsync(cancellationToken);
    }

    private async Task<SchoolBulkImportRequest?> WaitForImportRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < TimeSpan.FromSeconds(15))
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = await dbContext.SchoolBulkImportRequests
                .Include(x => x.Failures)
                .SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);

            if (request?.Status is SchoolBulkImportStatus.Completed or SchoolBulkImportStatus.CompletedWithFailures or SchoolBulkImportStatus.Failed)
            {
                return request;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }

        return null;
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
