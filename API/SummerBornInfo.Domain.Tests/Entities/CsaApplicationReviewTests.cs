namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class CsaApplicationReviewTests
{
    [Fact]
    public void GivenValidSubmission_WhenSubmitted_ThenReviewIsVisible()
    {
        var submittedAtUtc = DateTimeOffset.UtcNow;

        var review = CsaApplicationReview.Submit(Guid.NewGuid(), "Parent", applicationSuccessful: true, "Helpful", submittedAtUtc);

        Assert.Equal(CsaApplicationReviewStatus.Visible, review.Status);
        Assert.True(review.IsVisible);
        Assert.Equal(submittedAtUtc, review.SubmittedAtUtc);
        Assert.Empty(review.Reports);
    }

    [Fact]
    public void GivenUnapprovedVisibleReview_WhenFirstReportIsAttached_ThenReviewIsPendingApproval()
    {
        var review = CreateReview();

        var attached = review.AttachReport("inappropriate", details: null, "reporter-1", DateTimeOffset.UtcNow);

        Assert.True(attached);
        Assert.Equal(CsaApplicationReviewStatus.PendingApproval, review.Status);
        Assert.False(review.IsVisible);
        _ = Assert.Single(review.Reports);
    }

    [Fact]
    public void GivenPendingReview_WhenApproved_ThenReportsAreResolvedAndReviewIsVisible()
    {
        var review = CreateReview();
        _ = review.AttachReport("inappropriate", details: null, "reporter-1", DateTimeOffset.UtcNow);
        var resolvedAtUtc = DateTimeOffset.UtcNow.AddMinutes(1);

        review.Approve(resolvedAtUtc);

        Assert.Equal(CsaApplicationReviewStatus.Approved, review.Status);
        Assert.True(review.IsVisible);
        Assert.Equal(resolvedAtUtc, review.Reports[0].ResolvedAtUtc);
        Assert.Equal(0, review.PostApprovalDistinctReportCount);
    }

    [Fact]
    public void GivenApprovedReview_WhenTenDistinctReportsAreAttached_ThenReviewIsPendingReapproval()
    {
        var review = CreateApprovedReview();

        for (var reporter = 1; reporter <= CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport("other", "details", $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}", DateTimeOffset.UtcNow.AddMinutes(reporter));
        }

        Assert.Equal(CsaApplicationReviewStatus.PendingReapproval, review.Status);
        Assert.False(review.IsVisible);
        Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold, review.PostApprovalDistinctReportCount);
    }

    [Fact]
    public void GivenApprovedReview_WhenNineDistinctReportsAreAttached_ThenReviewRemainsVisible()
    {
        var review = CreateApprovedReview();

        for (var reporter = 1; reporter < CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport("other", "details", $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}", DateTimeOffset.UtcNow.AddMinutes(reporter));
        }

        Assert.Equal(CsaApplicationReviewStatus.Approved, review.Status);
        Assert.True(review.IsVisible);
        Assert.Equal(CsaApplicationReview.PostApprovalReportThreshold - 1, review.PostApprovalDistinctReportCount);
    }

    [Fact]
    public void GivenApprovedReviewAndDuplicateOpenFingerprint_WhenReportIsAttached_ThenDuplicateDoesNotCountOrPersist()
    {
        var review = CreateApprovedReview();
        _ = review.AttachReport("other", "first", "same-reporter", DateTimeOffset.UtcNow);

        var attached = review.AttachReport("other", "duplicate", "same-reporter", DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.False(attached);
        _ = Assert.Single(review.Reports, x => x.ResolvedAtUtc is null);
        Assert.Equal(1, review.PostApprovalDistinctReportCount);
    }

    [Fact]
    public void GivenApprovedReviewAndWhitespaceFingerprint_WhenReportIsAttached_ThenFingerprintIsStoredAsNull()
    {
        var review = CreateApprovedReview();

        var attached = review.AttachReport("other", "first", "   ", DateTimeOffset.UtcNow);

        Assert.True(attached);
        var report = Assert.Single(review.Reports, x => x.ResolvedAtUtc is null);
        Assert.Null(report.ReporterFingerprint);
    }

    [Fact]
    public void GivenPendingReapprovalReview_WhenApproved_ThenOpenReportsAreResolvedAndCountResets()
    {
        var review = CreateApprovedReview();
        for (var reporter = 1; reporter <= CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport("other", details: null, $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}", DateTimeOffset.UtcNow);
        }

        var resolvedAtUtc = DateTimeOffset.UtcNow.AddHours(1);
        review.Approve(resolvedAtUtc);

        Assert.Equal(CsaApplicationReviewStatus.Approved, review.Status);
        Assert.True(review.IsVisible);
        Assert.Equal(0, review.PostApprovalDistinctReportCount);
        Assert.All(
            review.Reports.Where(report => !string.Equals(report.ReporterFingerprint, "initial-reporter", StringComparison.Ordinal)),
            report => Assert.Equal(resolvedAtUtc, report.ResolvedAtUtc));
    }

    [Fact]
    public void GivenPendingReapprovalReview_WhenRejected_ThenOpenReportsAreResolvedAndReviewIsRejected()
    {
        var review = CreateApprovedReview();
        for (var reporter = 1; reporter <= CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport("other", details: null, $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}", DateTimeOffset.UtcNow);
        }

        var resolvedAtUtc = DateTimeOffset.UtcNow.AddHours(1);
        review.Reject(resolvedAtUtc);

        Assert.Equal(CsaApplicationReviewStatus.Rejected, review.Status);
        Assert.False(review.IsVisible);
        Assert.All(
            review.Reports.Where(report => !string.Equals(report.ReporterFingerprint, "initial-reporter", StringComparison.Ordinal)),
            report => Assert.Equal(resolvedAtUtc, report.ResolvedAtUtc));
    }

    [Fact]
    public void GivenApprovedReviewAndResolvedFingerprint_WhenSameFingerprintReportsAgain_ThenNewReportCounts()
    {
        var review = CreateApprovedReview();
        _ = review.AttachReport("other", "first", "same-reporter", DateTimeOffset.UtcNow);
        for (var reporter = 1; reporter < CsaApplicationReview.PostApprovalReportThreshold; reporter++)
        {
            _ = review.AttachReport("other", details: null, $"reporter-{reporter.ToString(CultureInfo.InvariantCulture)}", DateTimeOffset.UtcNow.AddMinutes(reporter + 1));
        }

        review.Approve(DateTimeOffset.UtcNow.AddMinutes(20));

        var attached = review.AttachReport("other", "second", "same-reporter", DateTimeOffset.UtcNow.AddMinutes(21));

        Assert.True(attached);
        Assert.Equal(1, review.PostApprovalDistinctReportCount);
        _ = Assert.Single(review.Reports, x => x.ResolvedAtUtc is null && string.Equals(x.ReporterFingerprint, "same-reporter", StringComparison.Ordinal));
    }

    private static CsaApplicationReview CreateReview()
    {
        return CsaApplicationReview.Submit(Guid.NewGuid(), "Parent", applicationSuccessful: true, "Helpful", DateTimeOffset.UtcNow);
    }

    private static CsaApplicationReview CreateApprovedReview()
    {
        var review = CreateReview();
        _ = review.AttachReport("inappropriate", details: null, "initial-reporter", DateTimeOffset.UtcNow);
        review.Approve(DateTimeOffset.UtcNow.AddMinutes(1));
        return review;
    }
}
