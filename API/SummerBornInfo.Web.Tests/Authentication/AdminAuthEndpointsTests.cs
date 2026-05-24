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
        _ = await SeedUserAsync(email, password, isAdmin: true);
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
        _ = await SeedUserAsync(email, "P@ssword123!", isAdmin: true);
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
        _ = await SeedUserAsync(email, password, isAdmin: false);
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
        _ = await SeedUserAsync(email, password, isAdmin: true);
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
