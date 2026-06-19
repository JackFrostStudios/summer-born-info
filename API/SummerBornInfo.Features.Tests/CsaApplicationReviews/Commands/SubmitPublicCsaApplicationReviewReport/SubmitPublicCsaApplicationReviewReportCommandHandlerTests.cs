namespace SummerBornInfo.Features.Tests.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReviewReport;

public sealed class SubmitPublicCsaApplicationReviewReportCommandHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenVisibleReview_WhenExecuteAsync_ThenPersistsReportAndMovesReviewToPendingApproval()
    {
        var school = SchoolFactory.GetSchool();
        var review = CsaApplicationReview.Submit(school.Id, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
        await SeedSchoolAndReviewAsync(school, review);

        var handler = new SubmitPublicCsaApplicationReviewReportCommandHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new SubmitPublicCsaApplicationReviewReportCommand(
                SchoolId: school.Id,
                ReviewId: review.Id,
                Reason: "spam",
                Details: "Repeated promotional content.",
                ReporterFingerprint: "fingerprint-1"),
            TestContext.Current.CancellationToken);

        Assert.Equal(SubmitPublicCsaApplicationReviewReportExecutionStatus.Accepted, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal(review.Id, result.Response.Id);

        var savedReview = await CreateDbContext().CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.PendingApproval, savedReview.Status);
        Assert.Equal("fingerprint-1", Assert.Single(savedReview.Reports).ReporterFingerprint);
    }

    [Fact]
    public async Task GivenApprovedReviewWithDuplicateOpenFingerprint_WhenExecuteAsync_ThenReturnsAcceptedWithoutPersistingSecondReport()
    {
        var school = SchoolFactory.GetSchool();
        var review = CreateApprovedReview(school.Id);
        _ = review.AttachReport("abusive", "First report.", "fingerprint-1", DateTimeOffset.UtcNow.AddMinutes(3));
        await SeedSchoolAndReviewAsync(school, review);

        var handler = new SubmitPublicCsaApplicationReviewReportCommandHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new SubmitPublicCsaApplicationReviewReportCommand(
                SchoolId: school.Id,
                ReviewId: review.Id,
                Reason: "privacy",
                Details: "Duplicate report.",
                ReporterFingerprint: "fingerprint-1"),
            TestContext.Current.CancellationToken);

        Assert.Equal(SubmitPublicCsaApplicationReviewReportExecutionStatus.Accepted, result.Status);

        var savedReview = await CreateDbContext().CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.Approved, savedReview.Status);
        Assert.Equal(2, savedReview.Reports.Count);
        Assert.Equal(1, savedReview.Reports.Count(x => x.ResolvedAtUtc is null));
    }

    [Theory]
    [InlineData(true, SubmitPublicCsaApplicationReviewReportExecutionStatus.SchoolNotFound)]
    [InlineData(false, SubmitPublicCsaApplicationReviewReportExecutionStatus.ReviewNotFound)]
    public async Task GivenUnknownSchoolOrReview_WhenExecuteAsync_ThenReturnsNotFoundStatus(
        bool useUnknownSchool,
        SubmitPublicCsaApplicationReviewReportExecutionStatus expectedStatus)
    {
        var school = SchoolFactory.GetSchool();
        var review = CsaApplicationReview.Submit(school.Id, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
        await SeedSchoolAndReviewAsync(school, review);

        var handler = new SubmitPublicCsaApplicationReviewReportCommandHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new SubmitPublicCsaApplicationReviewReportCommand(
                SchoolId: useUnknownSchool ? Guid.NewGuid() : school.Id,
                ReviewId: useUnknownSchool ? review.Id : Guid.NewGuid(),
                Reason: "spam",
                Details: "Repeated promotional content.",
                ReporterFingerprint: "fingerprint-1"),
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Null(result.Response);
    }

    private async Task SeedSchoolAndReviewAsync(School school, CsaApplicationReview review)
    {
        var dbContext = CreateDbContext();
        _ = dbContext.Schools.Add(school);
        _ = dbContext.CsaApplicationReviews.Add(review);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static CsaApplicationReview CreateApprovedReview(Guid schoolId)
    {
        var review = CsaApplicationReview.Submit(schoolId, "Parent", applicationSuccessful: true, "Helpful review.", DateTimeOffset.UtcNow);
        _ = review.AttachReport("spam", details: null, "initial-reporter", DateTimeOffset.UtcNow.AddMinutes(1));
        review.Approve(DateTimeOffset.UtcNow.AddMinutes(2));
        return review;
    }
}
