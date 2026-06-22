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
            $"/api/admin/csa-application-reviews/{Guid.CreateVersion7()}/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedNonAdminCaller_WhenModerationPosted_ThenReturnsForbidden()
    {
        var client = await CreateAuthenticatedTestClientAsync("volunteer@example.com", "P@ssword123!");

        var response = await client.PostAsJsonAsync(
            $"/api/admin/csa-application-reviews/{Guid.CreateVersion7()}/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenUnsupportedDecisionPosted_ThenReturnsBadRequest()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreatePendingApprovalReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.PostAsJsonAsync(
            $"/api/admin/csa-application-reviews/{review.Id}/moderation",
            new ModerateCsaApplicationReviewRequest("hold", "Needs follow-up."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Invalid CSA application review moderation request.", problem.Title);
        Assert.Contains("decision", problem.Errors.Keys, StringComparer.Ordinal);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenReviewNotFound_ThenReturnsNotFound()
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.PostAsJsonAsync(
            $"/api/admin/csa-application-reviews/{Guid.CreateVersion7()}/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "CSA application review not found.");
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenVisibleReviewModerated_ThenReturnsConflict()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreateVisibleReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.PostAsJsonAsync(
            $"/api/admin/csa-application-reviews/{review.Id}/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Conflict, "Invalid CSA application review moderation request.");
    }

    [Fact]
    public async Task GivenPendingApprovalReview_WhenApproved_ThenReturnsOkAndMakesReviewVisible()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreatePendingApprovalReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");
        var startedAtUtc = DateTimeOffset.UtcNow;

        var response = await client.PostAsJsonAsync(
            $"/api/admin/csa-application-reviews/{review.Id}/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var moderationResponse = await response.Content.ReadFromJsonAsync<ModerateCsaApplicationReviewResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(moderationResponse);
        Assert.Equal(review.Id, moderationResponse.Id);
        Assert.Equal("approved", moderationResponse.Status);
        Assert.Equal("Looks valid.", moderationResponse.ModeratorNote);
        Assert.InRange(
            moderationResponse.ModeratedAtUtc,
            startedAtUtc.AddSeconds(-1),
            DateTimeOffset.UtcNow.AddSeconds(1));

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.Approved, savedReview.Status);
        Assert.All(savedReview.Reports, report => Assert.NotNull(report.ResolvedAtUtc));
    }

    [Fact]
    public async Task GivenPendingReapprovalReview_WhenApproved_ThenReturnsOkResolvesReportsAndResetsCounter()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreatePendingReapprovalReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.PostAsJsonAsync(
            $"/api/admin/csa-application-reviews/{review.Id}/moderation",
            new ModerateCsaApplicationReviewRequest("approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.Approved, savedReview.Status);
        Assert.Equal(0, savedReview.PostApprovalDistinctReportCount);
        Assert.All(savedReview.Reports, report => Assert.NotNull(report.ResolvedAtUtc));
    }

    [Fact]
    public async Task GivenPendingReapprovalReview_WhenRejected_ThenReturnsOkKeepsReviewHiddenAndResolvesReports()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreatePendingReapprovalReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.PostAsJsonAsync(
            $"/api/admin/csa-application-reviews/{review.Id}/moderation",
            new ModerateCsaApplicationReviewRequest("reject", "Contains personal information."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.Rejected, savedReview.Status);
        Assert.False(savedReview.IsVisible);
        Assert.All(savedReview.Reports, report => Assert.NotNull(report.ResolvedAtUtc));
    }

    private async Task SeedSchoolAndReviewsAsync(School school, params CsaApplicationReview[] reviews)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _ = dbContext.Schools.Add(school);
        dbContext.CsaApplicationReviews.AddRange(reviews);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static CsaApplicationReview CreateVisibleReview(Guid schoolId)
    {
        return CsaApplicationReview.Submit(schoolId, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
    }

    private static CsaApplicationReview CreatePendingApprovalReview(Guid schoolId)
    {
        var review = CreateVisibleReview(schoolId);
        _ = review.AttachReport("spam", "Repeated promotional content.", "initial-reporter", DateTimeOffset.UtcNow.AddMinutes(1));
        return review;
    }

    private static CsaApplicationReview CreatePendingReapprovalReview(Guid schoolId)
    {
        var review = CreatePendingApprovalReview(schoolId);
        review.Approve(DateTimeOffset.UtcNow.AddMinutes(2));

        for (var reporter = 1; reporter <= CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport(
                "abusive",
                $"Report {reporter.ToString(CultureInfo.InvariantCulture)}",
                $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}",
                DateTimeOffset.UtcNow.AddMinutes(reporter + 2));
        }

        return review;
    }

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal((int)expectedStatusCode, problem.Status);
        Assert.Equal(expectedTitle, problem.Title);
    }

    private sealed record ModerateCsaApplicationReviewRequest(string Decision, string? ModeratorNote);

    private sealed record ModerateCsaApplicationReviewResponse(
        Guid Id,
        string Status,
        DateTimeOffset ModeratedAtUtc,
        string? ModeratorNote);
}
