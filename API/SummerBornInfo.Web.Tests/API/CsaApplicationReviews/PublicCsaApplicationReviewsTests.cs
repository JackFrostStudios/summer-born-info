namespace SummerBornInfo.Web.Tests.API.CsaApplicationReviews;

public sealed class PublicCsaApplicationReviewsTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenExistingSchoolAndValidRequest_WhenReviewSubmitted_ThenPersistsVisibleReviewAndReturnsCreated()
    {
        var school = SchoolFactory.GetSchool();
        await SeedSchoolAsync(school);

        var client = Factory.CreateClient();
        var startedAtUtc = DateTimeOffset.UtcNow;

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Helpful review."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<SubmitCsaApplicationReviewResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(payload);
        Assert.Equal(school.Id, payload.SchoolId);
        Assert.Equal("visible", payload.Status);
        Assert.Equal("Parent", payload.Name);
        Assert.True(payload.ApplicationSuccessful);
        Assert.Equal("Helpful review.", payload.Comment);
        Assert.InRange(payload.SubmittedAtUtc, startedAtUtc.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1));

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedReview = await dbContext.CsaApplicationReviews.SingleAsync(x => x.Id == payload.Id, TestContext.Current.CancellationToken);
        Assert.Equal(school.Id, savedReview.SchoolId);
        Assert.Equal(CsaApplicationReviewStatus.Visible, savedReview.Status);
        Assert.Equal("Parent", savedReview.Name);
        Assert.True(savedReview.ApplicationSuccessful);
        Assert.Equal("Helpful review.", savedReview.Comment);
    }

    [Fact]
    public async Task GivenUnknownSchool_WhenReviewSubmitted_ThenReturnsNotFound()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/schools/{Guid.NewGuid()}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Helpful review."),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "School not found.");
    }

    [Fact]
    public async Task GivenInvalidReviewSubmission_WhenReviewSubmitted_ThenReturnsFieldValidationErrors()
    {
        var school = SchoolFactory.GetSchool();
        await SeedSchoolAsync(school);

        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            $"/api/schools/{school.Id}/csa-application-reviews",
            new SubmitCsaApplicationReviewRequest(
                Name: " ",
                ApplicationSuccessful: null,
                Comment: string.Empty),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Invalid CSA application review submission request.", problem.Title);
        Assert.Contains("name", problem.Errors.Keys, StringComparer.Ordinal);
        Assert.Contains("applicationSuccessful", problem.Errors.Keys, StringComparer.Ordinal);
        Assert.Contains("comment", problem.Errors.Keys, StringComparer.Ordinal);
    }

    [Fact]
    public async Task GivenVisibleAndHiddenReviews_WhenReviewsRequested_ThenReturnsOnlyVisibleReviewsNewestFirst()
    {
        var school = SchoolFactory.GetSchool();
        var visibleNewest = CreateReview(
            school.Id,
            name: "Newest",
            applicationSuccessful: true,
            comment: "Newest comment.",
            submittedAtUtc: new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero));
        var approvedOlder = CreateApprovedReview(
            school.Id,
            name: "Approved",
            applicationSuccessful: false,
            comment: "Approved comment.",
            submittedAtUtc: new DateTimeOffset(2026, 6, 18, 12, 0, 0, TimeSpan.Zero));
        var hidden = CreatePendingApprovalReview(
            school.Id,
            name: "Hidden",
            applicationSuccessful: true,
            comment: "Hidden comment.",
            submittedAtUtc: new DateTimeOffset(2026, 6, 17, 12, 0, 0, TimeSpan.Zero));

        await SeedSchoolAndReviewsAsync(school, visibleNewest, approvedOlder, hidden);

        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/api/schools/{school.Id}/csa-application-reviews", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PublicCsaApplicationReviewsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(payload);
        Assert.Null(payload.NextCursor);
        Assert.Equal([visibleNewest.Id, approvedOlder.Id], [.. payload.Reviews.Select(x => x.Id)]);
        Assert.DoesNotContain(payload.Reviews, x => x.Id == hidden.Id);
    }

    [Fact]
    public async Task GivenUnknownSchool_WhenReviewsRequested_ThenReturnsNotFound()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/schools/{Guid.NewGuid()}/csa-application-reviews", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "School not found.");
    }

    [Fact]
    public async Task GivenMoreVisibleReviewsThanPageSize_WhenReviewsRequested_ThenReturnsStableCursorPagination()
    {
        var school = SchoolFactory.GetSchool();
        var sharedTimestamp = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var newest = CreateReview(
            school.Id,
            name: "Newest",
            applicationSuccessful: true,
            comment: "Newest comment.",
            submittedAtUtc: new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero),
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xC0));
        var tieBreakHigherId = CreateReview(
            school.Id,
            name: "Tie 2",
            applicationSuccessful: true,
            comment: "Tie comment 2.",
            submittedAtUtc: sharedTimestamp,
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xB0));
        var tieBreakLowerId = CreateReview(
            school.Id,
            name: "Tie 1",
            applicationSuccessful: false,
            comment: "Tie comment 1.",
            submittedAtUtc: sharedTimestamp,
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xA0));

        await SeedSchoolAndReviewsAsync(school, newest, tieBreakLowerId, tieBreakHigherId);

        var client = Factory.CreateClient();
        var firstResponse = await client.GetAsync(
            $"/api/schools/{school.Id}/csa-application-reviews?pageSize=2",
            TestContext.Current.CancellationToken);

        _ = firstResponse.EnsureSuccessStatusCode();
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<PublicCsaApplicationReviewsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(firstPage);
        Assert.Equal([newest.Id, tieBreakHigherId.Id], [.. firstPage.Reviews.Select(x => x.Id)]);
        Assert.NotNull(firstPage.NextCursor);
        Assert.DoesNotContain(tieBreakHigherId.Id.ToString(), firstPage.NextCursor, StringComparison.OrdinalIgnoreCase);

        var secondResponse = await client.GetAsync(
            $"/api/schools/{school.Id}/csa-application-reviews?pageSize=2&cursor={Uri.EscapeDataString(firstPage.NextCursor)}",
            TestContext.Current.CancellationToken);

        _ = secondResponse.EnsureSuccessStatusCode();
        var secondPage = await secondResponse.Content.ReadFromJsonAsync<PublicCsaApplicationReviewsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(secondPage);
        var remainingReview = Assert.Single(secondPage.Reviews);
        Assert.Equal(tieBreakLowerId.Id, remainingReview.Id);
        Assert.Null(secondPage.NextCursor);
    }

    [Theory]
    [InlineData("/api/schools/{0}/csa-application-reviews?pageSize=0")]
    [InlineData("/api/schools/{0}/csa-application-reviews?pageSize=201")]
    [InlineData("/api/schools/{0}/csa-application-reviews?cursor=not-a-valid-cursor")]
    public async Task GivenInvalidReviewListInput_WhenReviewsRequested_ThenReturnsBadRequest(string requestUriFormat)
    {
        var school = SchoolFactory.GetSchool();
        await SeedSchoolAsync(school);

        var client = Factory.CreateClient();
        var response = await client.GetAsync(
            string.Format(CultureInfo.InvariantCulture, requestUriFormat, school.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.BadRequest, "Invalid CSA application review request.");
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

    private static CsaApplicationReview CreateReview(
        Guid schoolId,
        string name,
        bool applicationSuccessful,
        string comment,
        DateTimeOffset submittedAtUtc,
        Guid? id = null)
    {
        var review = CsaApplicationReview.Submit(schoolId, name, applicationSuccessful, comment, submittedAtUtc);
        if (id.HasValue)
        {
            typeof(CsaApplicationReview).GetProperty(nameof(CsaApplicationReview.Id))!.SetValue(review, id.Value);
        }

        return review;
    }

    private static CsaApplicationReview CreateApprovedReview(
        Guid schoolId,
        string name,
        bool applicationSuccessful,
        string comment,
        DateTimeOffset submittedAtUtc)
    {
        var review = CreateReview(schoolId, name, applicationSuccessful, comment, submittedAtUtc);
        _ = review.AttachReport("other", details: null, "reporter-1", submittedAtUtc.AddMinutes(1));
        review.Approve(submittedAtUtc.AddMinutes(2));
        return review;
    }

    private static CsaApplicationReview CreatePendingApprovalReview(
        Guid schoolId,
        string name,
        bool applicationSuccessful,
        string comment,
        DateTimeOffset submittedAtUtc)
    {
        var review = CreateReview(schoolId, name, applicationSuccessful, comment, submittedAtUtc);
        _ = review.AttachReport("other", details: null, "reporter-1", submittedAtUtc.AddMinutes(1));
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

    private sealed record SubmitCsaApplicationReviewRequest(string? Name, bool? ApplicationSuccessful, string? Comment);

    private sealed record SubmitCsaApplicationReviewResponse(
        Guid Id,
        Guid SchoolId,
        string Name,
        bool ApplicationSuccessful,
        string Comment,
        string Status,
        DateTimeOffset SubmittedAtUtc);

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
