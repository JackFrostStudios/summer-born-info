namespace SummerBornInfo.Features.Tests.CsaApplicationReviews.Commands.Moderate;

public sealed class ModerateCsaApplicationReviewCommandHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenPendingApprovalReview_WhenApproved_ThenPersistsApprovedStatusAndResolvesOpenReports()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreatePendingApprovalReview(school.Id);
        await SeedSchoolAndReviewAsync(school, review);

        var handler = new ModerateCsaApplicationReviewCommandHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new ModerateCsaApplicationReviewCommand(review.Id, "approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(ModerateCsaApplicationReviewExecutionStatus.Succeeded, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal("approved", result.Response.Status);

        var savedReview = await CreateDbContext().CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.Approved, savedReview.Status);
        Assert.All(savedReview.Reports, report => Assert.NotNull(report.ResolvedAtUtc));
    }

    [Fact]
    public async Task GivenPendingReapprovalReview_WhenApproved_ThenPersistsApprovedStatusAndResetsPostApprovalCounter()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreatePendingReapprovalReview(school.Id);
        await SeedSchoolAndReviewAsync(school, review);

        var handler = new ModerateCsaApplicationReviewCommandHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new ModerateCsaApplicationReviewCommand(review.Id, "approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(ModerateCsaApplicationReviewExecutionStatus.Succeeded, result.Status);

        var savedReview = await CreateDbContext().CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.Approved, savedReview.Status);
        Assert.Equal(0, savedReview.PostApprovalDistinctReportCount);
    }

    [Theory]
    [InlineData("hold", ModerateCsaApplicationReviewExecutionStatus.InvalidDecision)]
    [InlineData("approve", ModerateCsaApplicationReviewExecutionStatus.ReviewNotFound)]
    public async Task GivenInvalidDecisionOrMissingReview_WhenExecuteAsync_ThenReturnsExpectedStatus(
        string decision,
        ModerateCsaApplicationReviewExecutionStatus expectedStatus)
    {
        var school = SchoolFactory.GetSchool();
        var review = CreatePendingApprovalReview(school.Id);
        await SeedSchoolAndReviewAsync(school, review);

        var handler = new ModerateCsaApplicationReviewCommandHandler(CreateDbContext());
        var reviewId = expectedStatus == ModerateCsaApplicationReviewExecutionStatus.ReviewNotFound
            ? Guid.CreateVersion7()
            : review.Id;

        var result = await handler.ExecuteAsync(
            new ModerateCsaApplicationReviewCommand(reviewId, decision, "Note"),
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task GivenVisibleReview_WhenModerated_ThenReturnsReviewNotPending()
    {
        var school = SchoolFactory.GetSchool();
        var review = CsaApplicationReview.Submit(school.Id, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
        await SeedSchoolAndReviewAsync(school, review);

        var handler = new ModerateCsaApplicationReviewCommandHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new ModerateCsaApplicationReviewCommand(review.Id, "approve", "Looks valid."),
            TestContext.Current.CancellationToken);

        Assert.Equal(ModerateCsaApplicationReviewExecutionStatus.ReviewNotPending, result.Status);
        Assert.Null(result.Response);
    }

    private async Task SeedSchoolAndReviewAsync(School school, CsaApplicationReview review)
    {
        var dbContext = CreateDbContext();
        _ = dbContext.Schools.Add(school);
        _ = dbContext.CsaApplicationReviews.Add(review);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static CsaApplicationReview CreatePendingApprovalReview(Guid schoolId)
    {
        var review = CsaApplicationReview.Submit(schoolId, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
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
}
