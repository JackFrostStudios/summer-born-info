namespace SummerBornInfo.Features.Tests.CsaApplicationReviews.Queries.GetPublicCsaApplicationReviews;

public sealed class GetPublicCsaApplicationReviewsQueryHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenVisibleAndHiddenReviews_WhenExecuteAsync_ThenReturnsOnlyVisibleReviewsNewestFirst()
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

        var handler = new GetPublicCsaApplicationReviewsQueryHandler(CreateDbContext());

        var response = await handler.ExecuteAsync(
            new GetPublicCsaApplicationReviewsQuery(SchoolId: school.Id, Cursor: null, PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Null(response.NextCursor);
        Assert.Equal([visibleNewest.Id, approvedOlder.Id], [.. response.Reviews.Select(x => x.Id)]);
    }

    [Fact]
    public async Task GivenUnknownSchool_WhenExecuteAsync_ThenReturnsNull()
    {
        var handler = new GetPublicCsaApplicationReviewsQueryHandler(CreateDbContext());

        var response = await handler.ExecuteAsync(
            new GetPublicCsaApplicationReviewsQuery(SchoolId: Guid.NewGuid(), Cursor: null, PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.Null(response);
    }

    [Fact]
    public async Task GivenSharedSubmissionTimestamp_WhenExecuteAsync_ThenUsesDescendingIdAsStableTieBreaker()
    {
        var school = SchoolFactory.GetSchool();
        var submittedAtUtc = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var higherId = CreateReview(
            school.Id,
            name: "Higher",
            applicationSuccessful: true,
            comment: "Higher comment.",
            submittedAtUtc: submittedAtUtc,
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xB0));
        var lowerId = CreateReview(
            school.Id,
            name: "Lower",
            applicationSuccessful: false,
            comment: "Lower comment.",
            submittedAtUtc: submittedAtUtc,
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xA0));

        await SeedSchoolAndReviewsAsync(school, lowerId, higherId);

        var handler = new GetPublicCsaApplicationReviewsQueryHandler(CreateDbContext());

        var response = await handler.ExecuteAsync(
            new GetPublicCsaApplicationReviewsQuery(SchoolId: school.Id, Cursor: null, PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal([higherId.Id, lowerId.Id], [.. response.Reviews.Select(x => x.Id)]);
    }

    private async Task SeedSchoolAndReviewsAsync(School school, params CsaApplicationReview[] reviews)
    {
        var dbContext = CreateDbContext();
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
}
