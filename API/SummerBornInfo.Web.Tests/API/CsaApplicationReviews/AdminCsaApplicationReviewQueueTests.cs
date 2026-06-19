namespace SummerBornInfo.Web.Tests.API.CsaApplicationReviews;

public sealed class AdminCsaApplicationReviewQueueTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenUnauthenticatedCaller_WhenQueueRequested_ThenReturnsUnauthorized()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/admin/csa-application-reviews", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedNonAdminCaller_WhenQueueRequested_ThenReturnsForbidden()
    {
        var client = await CreateAuthenticatedTestClientAsync("volunteer@example.com", "P@ssword123!");

        var response = await client.GetAsync("/api/admin/csa-application-reviews", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenQueueRequestedWithoutFilters_ThenReturnsPendingApprovalAndPendingReapprovalItemsOnly()
    {
        var firstSchool = SchoolFactory.GetSchool();
        var secondSchool = SchoolFactory.GetSchool();
        var pendingApprovalReview = CreatePendingApprovalReview(firstSchool.Id, "Parent one", DateTimeOffset.UtcNow.AddMinutes(-15));
        var pendingReapprovalReview = CreatePendingReapprovalReview(secondSchool.Id, "Parent two", DateTimeOffset.UtcNow.AddMinutes(-10));
        var approvedReview = CreateApprovedReview(firstSchool.Id, "Approved", DateTimeOffset.UtcNow.AddMinutes(-5));
        await SeedSchoolsAndReviewsAsync([firstSchool, secondSchool], pendingApprovalReview, pendingReapprovalReview, approvedReview);
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.GetAsync("/api/admin/csa-application-reviews", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AdminCsaApplicationReviewQueueResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.Equal(2, payload.Reviews.Count);
        Assert.Collection(
            payload.Reviews,
            first =>
            {
                Assert.Equal(pendingReapprovalReview.Id, first.Id);
                Assert.Equal("pendingReapproval", first.Status);
                Assert.Equal(secondSchool.Id, first.School.Id);
                Assert.Equal(secondSchool.URN, first.School.Urn);
                Assert.Equal(secondSchool.Name, first.School.Name);
                Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold, first.OpenReportCount);
                Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold, first.PostApprovalDistinctReportCount);
                Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold, first.Reports.Count);
            },
            second =>
            {
                Assert.Equal(pendingApprovalReview.Id, second.Id);
                Assert.Equal("pendingApproval", second.Status);
                Assert.Equal(0, second.PostApprovalDistinctReportCount);
                _ = Assert.Single(second.Reports);
                Assert.Equal("spam", second.Reports[0].Reason);
            });
        Assert.Null(payload.NextCursor);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenQueueRequestedWithFilterAndPaging_ThenReturnsMatchingPageAndCursor()
    {
        var school = SchoolFactory.GetSchool();
        var firstReview = CreatePendingReapprovalReview(school.Id, "First", DateTimeOffset.UtcNow.AddMinutes(-30));
        var secondReview = CreatePendingReapprovalReview(school.Id, "Second", DateTimeOffset.UtcNow.AddMinutes(-20));
        var pendingApprovalReview = CreatePendingApprovalReview(school.Id, "Third", DateTimeOffset.UtcNow.AddMinutes(-10));
        await SeedSchoolsAndReviewsAsync([school], firstReview, secondReview, pendingApprovalReview);
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var firstResponse = await client.GetAsync(
            "/api/admin/csa-application-reviews?queueState=PendingReapproval&pageSize=1",
            TestContext.Current.CancellationToken);

        _ = firstResponse.EnsureSuccessStatusCode();

        var firstPage = await firstResponse.Content.ReadFromJsonAsync<AdminCsaApplicationReviewQueueResponse>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(firstPage);
        _ = Assert.Single(firstPage.Reviews);
        Assert.Equal(secondReview.Id, firstPage.Reviews[0].Id);
        Assert.False(string.IsNullOrWhiteSpace(firstPage.NextCursor));

        var secondResponse = await client.GetAsync(
            $"/api/admin/csa-application-reviews?queueState=PendingReapproval&pageSize=1&cursor={Uri.EscapeDataString(firstPage.NextCursor)}",
            TestContext.Current.CancellationToken);

        _ = secondResponse.EnsureSuccessStatusCode();

        var secondPage = await secondResponse.Content.ReadFromJsonAsync<AdminCsaApplicationReviewQueueResponse>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(secondPage);
        _ = Assert.Single(secondPage.Reviews);
        Assert.Equal(firstReview.Id, secondPage.Reviews[0].Id);
        Assert.Null(secondPage.NextCursor);
    }

    [Theory]
    [InlineData("/api/admin/csa-application-reviews?pageSize=0")]
    [InlineData("/api/admin/csa-application-reviews?queueState=Approved")]
    [InlineData("/api/admin/csa-application-reviews?cursor=not-a-valid-cursor")]
    public async Task GivenInvalidQueueRequest_WhenQueueRequested_ThenReturnsBadRequest(string requestUri)
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.GetAsync(requestUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.BadRequest, "Invalid CSA application review moderation queue request.");
    }

    private async Task SeedSchoolsAndReviewsAsync(IReadOnlyCollection<School> schools, params CsaApplicationReview[] reviews)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Schools.AddRange(schools);
        dbContext.CsaApplicationReviews.AddRange(reviews);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static CsaApplicationReview CreatePendingApprovalReview(Guid schoolId, string reviewerName, DateTimeOffset submittedAtUtc)
    {
        var review = CsaApplicationReview.Submit(schoolId, reviewerName, applicationSuccessful: true, "Helpful review.", submittedAtUtc);
        _ = review.AttachReport("spam", "Repeated promotional content.", "initial-reporter", submittedAtUtc.AddMinutes(1));
        return review;
    }

    private static CsaApplicationReview CreateApprovedReview(Guid schoolId, string reviewerName, DateTimeOffset submittedAtUtc)
    {
        var review = CreatePendingApprovalReview(schoolId, reviewerName, submittedAtUtc);
        review.Approve(submittedAtUtc.AddMinutes(2));
        return review;
    }

    private static CsaApplicationReview CreatePendingReapprovalReview(Guid schoolId, string reviewerName, DateTimeOffset submittedAtUtc)
    {
        var review = CreateApprovedReview(schoolId, reviewerName, submittedAtUtc);
        for (var reporter = 1; reporter <= CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport(
                "abusive",
                $"Report {reporter.ToString(CultureInfo.InvariantCulture)}",
                $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}",
                submittedAtUtc.AddMinutes(reporter + 2));
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

    private sealed record AdminCsaApplicationReviewQueueResponse(
        IReadOnlyList<AdminCsaApplicationReviewQueueItemResponse> Reviews,
        string? NextCursor);

    private sealed record AdminCsaApplicationReviewQueueItemResponse(
        Guid Id,
        string ReviewerName,
        bool ApplicationSuccessful,
        string Comment,
        string Status,
        DateTimeOffset SubmittedAtUtc,
        int OpenReportCount,
        int PostApprovalDistinctReportCount,
        DateTimeOffset LatestReportAtUtc,
        AdminCsaApplicationReviewQueueSchoolResponse School,
        IReadOnlyList<AdminCsaApplicationReviewQueueReportResponse> Reports);

    private sealed record AdminCsaApplicationReviewQueueSchoolResponse(Guid Id, int Urn, string Name);

    private sealed record AdminCsaApplicationReviewQueueReportResponse(
        Guid Id,
        string Reason,
        string? Details,
        DateTimeOffset SubmittedAtUtc);
}
