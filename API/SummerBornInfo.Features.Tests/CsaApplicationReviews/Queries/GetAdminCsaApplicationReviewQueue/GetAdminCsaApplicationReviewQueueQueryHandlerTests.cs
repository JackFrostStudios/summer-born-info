namespace SummerBornInfo.Features.Tests.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public sealed class GetAdminCsaApplicationReviewQueueQueryHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenPendingAndApprovedReviews_WhenExecuteAsync_ThenReturnsPendingReviewsOrderedByLatestOpenReport()
    {
        var school = SchoolFactory.GetSchool();
        var pendingApprovalReview = CreatePendingApprovalReview(school.Id, "Pending approval", DateTimeOffset.UtcNow.AddMinutes(-20));
        var pendingReapprovalReview = CreatePendingReapprovalReview(school.Id, "Pending reapproval", DateTimeOffset.UtcNow.AddMinutes(-10));
        var approvedReview = CreateApprovedReview(school.Id, "Approved", DateTimeOffset.UtcNow.AddMinutes(-5));
        await SeedSchoolAndReviewsAsync(school, pendingApprovalReview, pendingReapprovalReview, approvedReview);

        var handler = new GetAdminCsaApplicationReviewQueueQueryHandler(CreateDbContext());

        var response = await handler.ExecuteAsync(
            new GetAdminCsaApplicationReviewQueueQuery(
                [
                    CsaApplicationReviewStatus.PendingApproval,
                    CsaApplicationReviewStatus.PendingReapproval,
                ],
                Cursor: null,
                PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.Collection(
            response.Reviews,
            first =>
            {
                Assert.Equal(pendingReapprovalReview.Id, first.Id);
                Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold, first.PostApprovalDistinctReportCount);
            },
            second =>
            {
                Assert.Equal(pendingApprovalReview.Id, second.Id);
                Assert.Equal(0, second.PostApprovalDistinctReportCount);
            });
        Assert.All(response.Reviews, review => Assert.NotEmpty(review.Reports));
        Assert.DoesNotContain(response.Reviews, review => review.Id == approvedReview.Id);
        Assert.Null(response.NextCursor);
    }

    [Fact]
    public async Task GivenFilteredQueueWithPageSizeOne_WhenExecuteAsync_ThenReturnsCursorForNextPage()
    {
        var school = SchoolFactory.GetSchool();
        var firstReview = CreatePendingReapprovalReview(school.Id, "First", DateTimeOffset.UtcNow.AddMinutes(-20));
        var secondReview = CreatePendingReapprovalReview(school.Id, "Second", DateTimeOffset.UtcNow.AddMinutes(-10));
        await SeedSchoolAndReviewsAsync(school, firstReview, secondReview);

        var handler = new GetAdminCsaApplicationReviewQueueQueryHandler(CreateDbContext());

        var firstResponse = await handler.ExecuteAsync(
            new GetAdminCsaApplicationReviewQueueQuery(
                [CsaApplicationReviewStatus.PendingReapproval],
                Cursor: null,
                PageSize: 1),
            TestContext.Current.CancellationToken);

        _ = Assert.Single(firstResponse.Reviews);
        Assert.Equal(secondReview.Id, firstResponse.Reviews[0].Id);
        Assert.False(string.IsNullOrWhiteSpace(firstResponse.NextCursor));

        var secondResponse = await handler.ExecuteAsync(
            new GetAdminCsaApplicationReviewQueueQuery(
                [CsaApplicationReviewStatus.PendingReapproval],
                firstResponse.NextCursor,
                PageSize: 1),
            TestContext.Current.CancellationToken);

        _ = Assert.Single(secondResponse.Reviews);
        Assert.Equal(firstReview.Id, secondResponse.Reviews[0].Id);
        Assert.Null(secondResponse.NextCursor);
    }

    private async Task SeedSchoolAndReviewsAsync(School school, params CsaApplicationReview[] reviews)
    {
        var dbContext = CreateDbContext();
        _ = dbContext.Schools.Add(school);
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
}
