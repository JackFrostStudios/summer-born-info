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
    [InlineData("/api/admin/school-imports", "post")]
    [InlineData("/api/admin/csa-application-reviews", "get")]
    [InlineData("/api/admin/csa-application-reviews/{reviewId}/moderation", "post")]
    public async Task GivenProtectedAdminOperation_WhenFetchedFromOpenApi_ThenSecurityAndErrorResponsesArePresent(string path, string method)
    {
        using var document = await GetOpenApiDocumentAsync();
        var operation = GetOperation(document, path, method);

        Assert.True(operation.TryGetProperty("security", out var security));
        Assert.Contains(
            security.EnumerateArray(),
            requirement => requirement.TryGetProperty(
                AdminSecurityOpenApiOptionsExtensions.IdentityApplicationCookieSecuritySchemeName,
                out var scopes)
                && scopes.ValueKind == JsonValueKind.Array);

        var responses = operation.GetProperty("responses");
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

    [Fact]
    public async Task GivenMilestoneFivePublicSubmissionOperation_WhenFetchedFromOpenApi_ThenRequestSchemaAndResponsesMatchDocumentedContract()
    {
        using var document = await GetOpenApiDocumentAsync();
        var operation = GetOperation(document, "/api/schools/{schoolId}/csa-application-reviews", "post");

        var requestSchema = ResolveSchema(
            document,
            operation.GetProperty("requestBody")
                .GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema"));

        var requestProperties = requestSchema.GetProperty("properties");
        Assert.True(requestProperties.TryGetProperty("name", out _));
        Assert.True(requestProperties.TryGetProperty("applicationSuccessful", out _));
        Assert.True(requestProperties.TryGetProperty("comment", out _));
        Assert.True(requestProperties.TryGetProperty("botVerificationToken", out _));

        var createdResponseSchema = ResolveSchema(
            document,
            operation.GetProperty("responses")
                .GetProperty("201")
                .GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema"));
        var createdProperties = createdResponseSchema.GetProperty("properties");
        Assert.True(createdProperties.TryGetProperty("id", out _));
        Assert.True(createdProperties.TryGetProperty("schoolId", out _));
        Assert.True(createdProperties.TryGetProperty("status", out _));
        Assert.True(createdProperties.TryGetProperty("submittedAtUtc", out _));

        var responses = operation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("400", out _));
        Assert.True(responses.TryGetProperty("404", out _));
        Assert.True(responses.TryGetProperty("429", out _));
    }

    [Fact]
    public async Task GivenMilestoneFivePublicReviewListOperation_WhenFetchedFromOpenApi_ThenPaginationShapeMatchesImplementation()
    {
        using var document = await GetOpenApiDocumentAsync();
        var operation = GetOperation(document, "/api/schools/{schoolId}/csa-application-reviews", "get");

        var responseSchema = ResolveSchema(
            document,
            operation.GetProperty("responses")
                .GetProperty("200")
                .GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema"));
        var properties = responseSchema.GetProperty("properties");

        Assert.True(properties.TryGetProperty("reviews", out var reviewsProperty));
        Assert.True(properties.TryGetProperty("nextCursor", out _));
        Assert.False(properties.TryGetProperty("items", out _));
        Assert.False(properties.TryGetProperty("hasMore", out _));

        var reviewItemSchema = ResolveSchema(document, reviewsProperty.GetProperty("items"));
        var reviewItemProperties = reviewItemSchema.GetProperty("properties");
        Assert.True(reviewItemProperties.TryGetProperty("id", out _));
        Assert.True(reviewItemProperties.TryGetProperty("name", out _));
        Assert.True(reviewItemProperties.TryGetProperty("applicationSuccessful", out _));
        Assert.True(reviewItemProperties.TryGetProperty("comment", out _));
        Assert.True(reviewItemProperties.TryGetProperty("submittedAtUtc", out _));

        var responses = operation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("400", out _));
        Assert.True(responses.TryGetProperty("404", out _));
    }

    [Fact]
    public async Task GivenMilestoneFiveReportAndModerationOperations_WhenFetchedFromOpenApi_ThenResponsesMatchImplementedWorkflow()
    {
        using var document = await GetOpenApiDocumentAsync();
        var reportOperation = GetOperation(document, "/api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports", "post");
        var reportResponses = reportOperation.GetProperty("responses");
        Assert.True(reportResponses.TryGetProperty("202", out _));
        Assert.True(reportResponses.TryGetProperty("400", out _));
        Assert.True(reportResponses.TryGetProperty("404", out _));
        Assert.True(reportResponses.TryGetProperty("429", out _));

        var moderationOperation = GetOperation(document, "/api/admin/csa-application-reviews/{reviewId}/moderation", "post");
        var moderationResponses = moderationOperation.GetProperty("responses");
        Assert.True(moderationResponses.TryGetProperty("200", out _));
        Assert.True(moderationResponses.TryGetProperty("400", out _));
        Assert.True(moderationResponses.TryGetProperty("401", out _));
        Assert.True(moderationResponses.TryGetProperty("403", out _));
        Assert.True(moderationResponses.TryGetProperty("404", out _));
        Assert.True(moderationResponses.TryGetProperty("409", out _));

        var queueOperation = GetOperation(document, "/api/admin/csa-application-reviews", "get");
        var queueSchema = ResolveSchema(
            document,
            queueOperation.GetProperty("responses")
                .GetProperty("200")
                .GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema"));
        var queueProperties = queueSchema.GetProperty("properties");
        Assert.True(queueProperties.TryGetProperty("reviews", out var queueReviewsProperty));
        Assert.True(queueProperties.TryGetProperty("nextCursor", out _));

        var queueItemSchema = ResolveSchema(document, queueReviewsProperty.GetProperty("items"));
        var queueItemProperties = queueItemSchema.GetProperty("properties");
        Assert.True(queueItemProperties.TryGetProperty("reviewerName", out _));
        Assert.True(queueItemProperties.TryGetProperty("status", out _));
        Assert.True(queueItemProperties.TryGetProperty("openReportCount", out _));
        Assert.True(queueItemProperties.TryGetProperty("postApprovalDistinctReportCount", out _));
        Assert.True(queueItemProperties.TryGetProperty("latestReportAtUtc", out _));
        Assert.True(queueItemProperties.TryGetProperty("school", out _));
        Assert.True(queueItemProperties.TryGetProperty("reports", out _));
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
        return GetOperation(document, path, "post");
    }

    private static JsonElement GetOperation(JsonDocument document, string path, string method)
    {
        return document.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty(method);
    }

    private static JsonElement ResolveSchema(JsonDocument document, JsonElement schema)
    {
        if (!schema.TryGetProperty("$ref", out var reference))
        {
            return schema;
        }

        var referenceValue = reference.GetString();
        Assert.NotNull(referenceValue);
        const string prefix = "#/components/schemas/";
        Assert.StartsWith(prefix, referenceValue, StringComparison.Ordinal);

        return document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(referenceValue[prefix.Length..]);
    }
}
