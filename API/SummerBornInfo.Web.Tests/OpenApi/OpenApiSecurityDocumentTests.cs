using SummerBornInfo.Web.OpenApi;

namespace SummerBornInfo.Web.Tests.OpenApi;

public sealed class OpenApiSecurityDocumentTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenOpenApiDocument_WhenFetched_ThenCookieSecuritySchemeExists()
    {
        using var document = await GetOpenApiDocumentAsync();
        var scheme = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty(AdminSecurityOpenApiOptionsExtensions.IdentityApplicationCookieSecuritySchemeName);

        Assert.Equal("apiKey", scheme.GetProperty("type").GetString());
        Assert.Equal("cookie", scheme.GetProperty("in").GetString());
        Assert.Equal(await GetApplicationCookieNameAsync(), scheme.GetProperty("name").GetString());
    }

    [Theory]
    [InlineData("/api/admin/school-imports")]
    [InlineData("/api/admin/csa-application-reviews/{reviewId}/moderation")]
    public async Task GivenProtectedAdminOperation_WhenFetchedFromOpenApi_ThenSecurityAndErrorResponsesArePresent(string path)
    {
        using var document = await GetOpenApiDocumentAsync();
        var postOperation = GetPostOperation(document, path);

        Assert.True(postOperation.TryGetProperty("security", out var security));
        Assert.Contains(
            security.EnumerateArray(),
            requirement => requirement.TryGetProperty(
                AdminSecurityOpenApiOptionsExtensions.IdentityApplicationCookieSecuritySchemeName,
                out var scopes)
                && scopes.ValueKind == JsonValueKind.Array);

        var responses = postOperation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("401", out _));
        Assert.True(responses.TryGetProperty("403", out _));
    }

    [Fact]
    public async Task GivenAllowAnonymousAdminAuthOperation_WhenFetchedFromOpenApi_ThenSecurityRequirementIsNotPresent()
    {
        using var document = await GetOpenApiDocumentAsync();
        var postOperation = GetPostOperation(document, "/api/admin/auth/sign-in");

        Assert.False(postOperation.TryGetProperty("security", out var security)
            && security.ValueKind == JsonValueKind.Array
            && security.GetArrayLength() > 0);
    }

    private async Task<JsonDocument> GetOpenApiDocumentAsync()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return await JsonDocument.ParseAsync(contentStream, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static JsonElement GetPostOperation(JsonDocument document, string path)
    {
        return document.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty("post");
    }
}
