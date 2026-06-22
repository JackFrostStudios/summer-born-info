namespace SummerBornInfo.Web.Tests.API.CsaApplicationReviews;

public sealed class SubmitPublicCsaApplicationReviewReportTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenVisibleReview_WhenFirstReportSubmitted_ThenReturnsAcceptedAndHidesReview()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreateVisibleReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);

        var client = CreateAnonymousReporterClient("203.0.113.10");
        var startedAtUtc = DateTimeOffset.UtcNow;

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest("spam", "Repeated promotional content."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<SubmitPublicCsaApplicationReviewReportResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(payload);
        Assert.Equal(review.Id, payload.Id);
        Assert.Equal("reportAccepted", payload.Status);
        Assert.InRange(payload.ReportedAtUtc, startedAtUtc.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1));

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.PendingApproval, savedReview.Status);
        var report = Assert.Single(savedReview.Reports);
        Assert.Equal("spam", report.Reason);
        Assert.Equal("Repeated promotional content.", report.Details);
        Assert.False(string.IsNullOrWhiteSpace(report.ReporterFingerprint));

        var publicReviewsResponse = await client.GetAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            TestContext.Current.CancellationToken);
        _ = publicReviewsResponse.EnsureSuccessStatusCode();

        var publicReviews = await publicReviewsResponse.Content.ReadFromJsonAsync<PublicCsaApplicationReviewsResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(publicReviews);
        Assert.DoesNotContain(publicReviews.Reviews, x => x.Id == review.Id);
    }

    [Fact]
    public async Task GivenApprovedReviewWithNineDistinctOpenReports_WhenTenthDistinctReportSubmitted_ThenReturnsAcceptedAndMovesToPendingReapproval()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreateApprovedReview(school.Id);
        for (var reporter = 1; reporter < CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport(
                "abusive",
                $"Report {reporter.ToString(CultureInfo.InvariantCulture)}",
                $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}",
                DateTimeOffset.UtcNow.AddMinutes(reporter));
        }

        await SeedSchoolAndReviewsAsync(school, review);

        var client = CreateAnonymousReporterClient("203.0.113.99");

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest("privacy", "Contains personal information."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.PendingReapproval, savedReview.Status);
        Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold, savedReview.Reports.Count(x => x.ResolvedAtUtc is null));
    }

    [Fact]
    public async Task GivenApprovedReviewAndDuplicateReporterFingerprint_WhenReportSubmittedTwice_ThenSecondRequestReturnsAcceptedWithoutCountingTwice()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreateApprovedReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);

        var client = CreateAnonymousReporterClient("203.0.113.55");

        var firstResponse = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest("abusive", "First report."),
            TestContext.Current.CancellationToken);
        var secondResponse = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest("privacy", "Duplicate open report."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, secondResponse.StatusCode);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.Approved, savedReview.Status);
        Assert.Equal(1, savedReview.Reports.Count(x => x.ResolvedAtUtc is null));
        Assert.Equal(2, savedReview.Reports.Count);
    }

    [Theory]
    [InlineData("other", null, "details")]
    [InlineData("unsupported", "Some details", "reason")]
    public async Task GivenInvalidReportRequest_WhenReportSubmitted_ThenReturnsBadRequest(
        string reason,
        string? details,
        string expectedErrorKey)
    {
        var school = SchoolFactory.GetSchool();
        var review = CreateVisibleReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);

        var client = CreateAnonymousReporterClient("203.0.113.77");

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest(reason, details),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Invalid CSA application review report request.", problem.Title);
        Assert.Contains(expectedErrorKey, problem.Errors.Keys, StringComparer.Ordinal);
    }

    [Fact]
    public async Task GivenUnknownSchool_WhenReportSubmitted_ThenReturnsNotFound()
    {
        var client = CreateAnonymousReporterClient("203.0.113.88");

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{Guid.NewGuid()}/csa-application-reviews/{Guid.NewGuid()}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest("spam", "Repeated promotional content."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "School not found.");
    }

    [Fact]
    public async Task GivenWrongSchoolRouteOrHiddenReview_WhenReportSubmitted_ThenReturnsNotFound()
    {
        var correctSchool = SchoolFactory.GetSchool();
        var otherSchool = SchoolFactory.GetSchool();
        var visibleReview = CreateVisibleReview(correctSchool.Id);
        var hiddenReview = CreatePendingApprovalReview(correctSchool.Id);
        await SeedSchoolsAndReviewsAsync([correctSchool, otherSchool], visibleReview, hiddenReview);

        var client = CreateAnonymousReporterClient("203.0.113.111");

        var wrongSchoolResponse = await client.PostAsJsonAsync(
            $"/api/schools/{otherSchool.Id}/csa-application-reviews/{visibleReview.Id}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest("spam", "Repeated promotional content."),
            TestContext.Current.CancellationToken);
        var hiddenReviewResponse = await client.PostAsJsonAsync(
            $"/api/schools/{correctSchool.Id}/csa-application-reviews/{hiddenReview.Id}/reports",
            new SubmitPublicCsaApplicationReviewReportRequest("spam", "Repeated promotional content."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, wrongSchoolResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, hiddenReviewResponse.StatusCode);
        await AssertProblemDetailsAsync(wrongSchoolResponse, HttpStatusCode.NotFound, "CSA application review not found.");
        await AssertProblemDetailsAsync(hiddenReviewResponse, HttpStatusCode.NotFound, "CSA application review not found.");
    }

    private HttpClient CreateAnonymousReporterClient(string forwardedFor)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", forwardedFor);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("summer-born-info-report-tests/1.0");
        return client;
    }

    private async Task SeedSchoolAndReviewsAsync(School school, params CsaApplicationReview[] reviews)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _ = dbContext.Schools.Add(school);
        dbContext.CsaApplicationReviews.AddRange(reviews);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task SeedSchoolsAndReviewsAsync(IReadOnlyCollection<School> schools, params CsaApplicationReview[] reviews)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Schools.AddRange(schools);
        dbContext.CsaApplicationReviews.AddRange(reviews);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static CsaApplicationReview CreateVisibleReview(Guid schoolId)
    {
        return CsaApplicationReview.Submit(schoolId, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
    }

    private static CsaApplicationReview CreateApprovedReview(Guid schoolId)
    {
        var review = CreateVisibleReview(schoolId);
        _ = review.AttachReport("spam", details: null, "initial-reporter", DateTimeOffset.UtcNow.AddMinutes(1));
        review.Approve(DateTimeOffset.UtcNow.AddMinutes(2));
        return review;
    }

    private static CsaApplicationReview CreatePendingApprovalReview(Guid schoolId)
    {
        var review = CreateVisibleReview(schoolId);
        _ = review.AttachReport("spam", details: null, "initial-reporter", DateTimeOffset.UtcNow.AddMinutes(1));
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

    private sealed record SubmitPublicCsaApplicationReviewReportRequest(string? Reason, string? Details);

    private sealed record SubmitPublicCsaApplicationReviewReportResponse(
        Guid Id,
        string Status,
        DateTimeOffset ReportedAtUtc);

    private sealed record PublicCsaApplicationReviewsResponse(
        IReadOnlyList<PublicCsaApplicationReviewResponse> Reviews,
        string? NextCursor);

    private sealed record PublicCsaApplicationReviewResponse(
        Guid Id,
        string Name,
        bool ApplicationSuccessful,
        string Comment,
        DateTimeOffset SubmittedAtUtc);
}
