namespace SummerBornInfo.Web.Tests.Authentication;

public sealed class AuthenticationRegistrationTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenHostServices_WhenResolved_ThenIdentityAndAdminAuthorizationAreRegistered()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        Assert.NotNull(serviceProvider.GetService<UserManager<ApplicationUser>>());
        Assert.NotNull(serviceProvider.GetService<SignInManager<ApplicationUser>>());
        Assert.NotNull(serviceProvider.GetService<RoleManager<ApplicationRole>>());

        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        Assert.Equal(IdentityConstants.ApplicationScheme, (await schemeProvider.GetDefaultAuthenticateSchemeAsync())?.Name);
        Assert.Equal(IdentityConstants.ApplicationScheme, (await schemeProvider.GetDefaultChallengeSchemeAsync())?.Name);
        Assert.Equal(IdentityConstants.ApplicationScheme, (await schemeProvider.GetDefaultForbidSchemeAsync())?.Name);

        var authorizationOptions = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var adminPolicy = authorizationOptions.GetPolicy(AdminAuthorizationPolicyNames.Admin);

        Assert.NotNull(adminPolicy);
        Assert.Contains(
            adminPolicy.Requirements.OfType<RolesAuthorizationRequirement>(),
            requirement => requirement.AllowedRoles.Contains(ApplicationRoleNames.Admin, StringComparer.Ordinal));
    }

    [Fact]
    public async Task GivenCookieAuthRedirectToLogin_WhenInvoked_ThenResponseIs401WithoutRedirect()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var options = optionsMonitor.Get(IdentityConstants.ApplicationScheme);
        var httpContext = new DefaultHttpContext();
        var redirectContext = CreateRedirectContext(httpContext, options);

        await options.Events.OnRedirectToLogin(redirectContext);

        Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
        Assert.False(httpContext.Response.Headers.ContainsKey("Location"));
    }

    [Fact]
    public async Task GivenCookieAuthRedirectToAccessDenied_WhenInvoked_ThenResponseIs403WithoutRedirect()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var options = optionsMonitor.Get(IdentityConstants.ApplicationScheme);
        var httpContext = new DefaultHttpContext();
        var redirectContext = CreateRedirectContext(httpContext, options);

        await options.Events.OnRedirectToAccessDenied(redirectContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
        Assert.False(httpContext.Response.Headers.ContainsKey("Location"));
    }

    private static RedirectContext<CookieAuthenticationOptions> CreateRedirectContext(
        HttpContext httpContext,
        CookieAuthenticationOptions options)
    {
        return new RedirectContext<CookieAuthenticationOptions>(
            httpContext,
            new AuthenticationScheme(
                IdentityConstants.ApplicationScheme,
                displayName: null,
                typeof(CookieAuthenticationHandler)),
            options,
            new AuthenticationProperties(),
            "https://example.test/redirect");
    }
}
