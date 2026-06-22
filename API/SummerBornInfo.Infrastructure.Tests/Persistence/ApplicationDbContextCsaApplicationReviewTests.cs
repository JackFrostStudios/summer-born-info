namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextCsaApplicationReviewTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenReviewWithReport_WhenPersisted_ThenAggregateStateRoundTrips()
    {
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        var review = CsaApplicationReview.Submit(school.Id, "Parent", applicationSuccessful: true, "Helpful", DateTimeOffset.UtcNow);
        _ = review.AttachReport("other", "details", "fingerprint", DateTimeOffset.UtcNow.AddMinutes(1));
        _ = dbContext.Schools.Add(school);
        _ = dbContext.CsaApplicationReviews.Add(review);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.ChangeTracker.Clear();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReviewStatus.PendingApproval, savedReview.Status);
        Assert.False(savedReview.IsVisible);
        var report = Assert.Single(savedReview.Reports);
        Assert.Equal("other", report.Reason);
        Assert.Equal("details", report.Details);
        Assert.Equal("fingerprint", report.ReporterFingerprint);
        Assert.Null(report.ResolvedAtUtc);
    }

    [Fact]
    public async Task GivenReviewForUnknownSchool_WhenPersisted_ThenForeignKeyConstraintRejectsIt()
    {
        var dbContext = CreateDbContext();
        var review = CsaApplicationReview.Submit(Guid.NewGuid(), "Parent", applicationSuccessful: false, "Comment", DateTimeOffset.UtcNow);
        _ = dbContext.CsaApplicationReviews.Add(review);

        _ = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenPersistedReview_WhenSchoolIsDeleted_ThenReviewAndReportsAreDeleted()
    {
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        var review = CsaApplicationReview.Submit(school.Id, "Parent", applicationSuccessful: true, "Comment", DateTimeOffset.UtcNow);
        _ = review.AttachReport("other", details: null, "fingerprint", DateTimeOffset.UtcNow);
        _ = dbContext.Schools.Add(school);
        _ = dbContext.CsaApplicationReviews.Add(review);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _ = await dbContext.Database.ExecuteSqlRawAsync(
            """DELETE FROM school WHERE "Id" = {0}""",
            [school.Id],
            TestContext.Current.CancellationToken);

        Assert.Empty(await dbContext.CsaApplicationReviews.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await dbContext.CsaApplicationReviewReports.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenApprovedReviewLoadedTwice_WhenSameOpenFingerprintIsSavedConcurrently_ThenFilteredUniqueIndexRejectsSecondSave()
    {
        var school = SchoolFactory.GetSchool();
        var seedContext = CreateDbContext();
        var seededReview = CreateApprovedReview(school.Id);
        _ = seedContext.Schools.Add(school);
        _ = seedContext.CsaApplicationReviews.Add(seededReview);
        _ = await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dbContextOne = CreateDbContext();
        var dbContextTwo = CreateDbContext();

        var reviewOne = await dbContextOne.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == seededReview.Id, TestContext.Current.CancellationToken);
        var reviewTwo = await dbContextTwo.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == seededReview.Id, TestContext.Current.CancellationToken);

        _ = reviewOne.AttachReport("other", "first", "same-fingerprint", DateTimeOffset.UtcNow);
        _ = reviewTwo.AttachReport("other", "second", "same-fingerprint", DateTimeOffset.UtcNow.AddMinutes(1));

        _ = await dbContextOne.SaveChangesAsync(TestContext.Current.CancellationToken);

        _ = await Assert.ThrowsAsync<DbUpdateException>(() => dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenApprovedReviewWithResolvedFingerprint_WhenSameFingerprintReportsAgain_ThenFilteredIndexAllowsNewOpenReport()
    {
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        var review = CreateApprovedReview(school.Id);
        _ = review.AttachReport("other", "first", "same-fingerprint", DateTimeOffset.UtcNow);
        for (var reporter = 1; reporter < CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport("other", details: null, $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}", DateTimeOffset.UtcNow.AddMinutes(reporter + 1));
        }

        review.Approve(DateTimeOffset.UtcNow.AddMinutes(20));
        _ = review.AttachReport("other", "second", "same-fingerprint", DateTimeOffset.UtcNow.AddMinutes(21));
        _ = dbContext.Schools.Add(school);
        _ = dbContext.CsaApplicationReviews.Add(review);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.ChangeTracker.Clear();
        var savedReview = await dbContext.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleAsync(x => x.Id == review.Id, TestContext.Current.CancellationToken);

        Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold + 2, savedReview.Reports.Count);
        Assert.Equal(2, savedReview.Reports.Count(x => string.Equals(x.ReporterFingerprint, "same-fingerprint", StringComparison.Ordinal)));
        _ = Assert.Single(savedReview.Reports, x => x.ResolvedAtUtc is null && string.Equals(x.ReporterFingerprint, "same-fingerprint", StringComparison.Ordinal));
    }

    private static CsaApplicationReview CreateApprovedReview(Guid schoolId)
    {
        var review = CsaApplicationReview.Submit(schoolId, "Parent", applicationSuccessful: true, "Helpful", DateTimeOffset.UtcNow);
        _ = review.AttachReport("other", details: null, "initial-reporter", DateTimeOffset.UtcNow.AddMinutes(1));
        review.Approve(DateTimeOffset.UtcNow.AddMinutes(2));
        return review;
    }
}
