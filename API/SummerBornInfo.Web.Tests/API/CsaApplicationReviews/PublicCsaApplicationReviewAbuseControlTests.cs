namespace SummerBornInfo.Web.Tests.API.CsaApplicationReviews;

public sealed class PublicCsaApplicationReviewAbuseControlTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(
        testDatabaseServerFixture,
        testOutputHelper,
        CreateAbuseControlConfiguration())
{
    [Fact]
    public async Task GivenBotVerificationDisabled_WhenReviewSubmittedWithoutToken_ThenReturnsCreated()
    {
        var school = SchoolFactory.GetSchool();
        await SeedSchoolAsync(school);

        var client = CreateAnonymousClient("203.0.113.200");

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Helpful review."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GivenBotVerificationDisabled_WhenReportSubmittedWithoutToken_ThenReturnsAccepted()
    {
        var school = SchoolFactory.GetSchool();
        var review = CsaApplicationReview.Submit(school.Id, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
        await SeedSchoolAndReviewsAsync(school, review);

        var client = CreateAnonymousClient("203.0.113.201");

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitCsaApplicationReviewReportRequest("spam", "Repeated promotional content."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task GivenSubmitRateLimitReachedByOneCaller_WhenDifferentCallerSubmitsReview_ThenUsesSeparatePartition()
    {
        var school = SchoolFactory.GetSchool();
        await SeedSchoolAsync(school);

        var firstCaller = CreateAnonymousClient("203.0.113.210");
        var secondCaller = CreateAnonymousClient("203.0.113.211");

        var firstResponse = await firstCaller.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: "Caller one",
                ApplicationSuccessful: true,
                Comment: "First comment."),
            TestContext.Current.CancellationToken);
        var secondResponse = await secondCaller.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: "Caller two",
                ApplicationSuccessful: false,
                Comment: "Second comment."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GivenSubmitRateLimitExceeded_WhenSameCallerSubmitsReviewTwice_ThenSecondRequestReturnsTooManyRequests()
    {
        var school = SchoolFactory.GetSchool();
        await SeedSchoolAsync(school);

        var client = CreateAnonymousClient("203.0.113.220");

        var firstResponse = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "First comment."),
            TestContext.Current.CancellationToken);
        var secondResponse = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Second comment."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        await AssertTooManyRequestsAsync(secondResponse);
    }

    [Fact]
    public async Task GivenReportRateLimitExceeded_WhenSameCallerSubmitsReportTwice_ThenSecondRequestReturnsTooManyRequests()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreateApprovedReview(school.Id);
        await SeedSchoolAndReviewsAsync(school, review);

        var client = CreateAnonymousClient("203.0.113.230");

        var firstResponse = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitCsaApplicationReviewReportRequest("spam", "First report."),
            TestContext.Current.CancellationToken);
        var secondResponse = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews/{review.Id}/reports",
            new SubmitCsaApplicationReviewReportRequest("privacy", "Second report."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, firstResponse.StatusCode);
        await AssertTooManyRequestsAsync(secondResponse);
    }

    private HttpClient CreateAnonymousClient(string forwardedFor)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", forwardedFor);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("summer-born-info-abuse-control-tests/1.0");
        return client;
    }

    private async Task SeedSchoolAsync(School school)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task SeedSchoolAndReviewsAsync(School school, params CsaApplicationReview[] reviews)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _ = dbContext.Schools.Add(school);
        dbContext.CsaApplicationReviews.AddRange(reviews);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static CsaApplicationReview CreateApprovedReview(Guid schoolId)
    {
        var review = CsaApplicationReview.Submit(schoolId, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
        _ = review.AttachReport("spam", details: null, "initial-reporter", DateTimeOffset.UtcNow.AddMinutes(1));
        review.Approve(DateTimeOffset.UtcNow.AddMinutes(2));
        return review;
    }

    private static async Task AssertTooManyRequestsAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status429TooManyRequests, problem.Status);
        Assert.Equal("Too many requests.", problem.Title);
    }

    private static Dictionary<string, string?> CreateAbuseControlConfiguration()
    {
        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["AbuseControls:BotVerification:Mode"] = "Disabled",
            ["AbuseControls:RateLimiting:ReviewSubmission:PermitLimit"] = "1",
            ["AbuseControls:RateLimiting:ReviewSubmission:QueueLimit"] = "0",
            ["AbuseControls:RateLimiting:ReviewSubmission:WindowSeconds"] = "60",
            ["AbuseControls:RateLimiting:ReviewReportSubmission:PermitLimit"] = "1",
            ["AbuseControls:RateLimiting:ReviewReportSubmission:QueueLimit"] = "0",
            ["AbuseControls:RateLimiting:ReviewReportSubmission:WindowSeconds"] = "60",
        };
    }

    private sealed record SubmitCsaApplicationReviewRequest(
        string? Name,
        bool? ApplicationSuccessful,
        string? Comment,
        string? BotVerificationToken = null);

    private sealed record SubmitCsaApplicationReviewReportRequest(
        string? Reason,
        string? Details,
        string? BotVerificationToken = null);
}
