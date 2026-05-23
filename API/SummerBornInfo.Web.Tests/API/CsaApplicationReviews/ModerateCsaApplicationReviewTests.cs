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
        var client = await CreateAuthenticatedTestClientAsync("volunteer@example.com", "P@ssword123!");

        var response = await client.PostAsJsonAsync(
            "/api/admin/csa-application-reviews/rev_123/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenUnsupportedDecisionPosted_ThenReturnsBadRequest()
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

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
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");
        var startedAtUtc = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync(
            "/api/admin/csa-application-reviews/rev_123/moderation",
            new ModerateCsaApplicationReviewRequest(decision, "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var moderationResponse = await response.Content.ReadFromJsonAsync<ModerateCsaApplicationReviewResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(moderationResponse);
        Assert.Equal("rev_123", moderationResponse.Id);
        Assert.Equal(expectedStatus, moderationResponse.Status);
        Assert.Equal("Looks valid.", moderationResponse.ModeratorNote);
        Assert.InRange(
            moderationResponse.ModeratedAtUtc,
            startedAtUtc.AddSeconds(-1),
            DateTime.UtcNow.AddSeconds(1));
    }

    private sealed record ModerateCsaApplicationReviewRequest(string Decision, string? ModeratorNote);
    private sealed record ModerateCsaApplicationReviewResponse(
        string Id,
        string Status,
        DateTime ModeratedAtUtc,
        string? ModeratorNote);
}
