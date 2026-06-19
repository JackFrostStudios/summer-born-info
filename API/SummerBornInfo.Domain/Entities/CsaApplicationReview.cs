namespace SummerBornInfo.Domain.Entities;

public sealed class CsaApplicationReview
{
    public const int PostApprovalReportThreshold = 10;

    private readonly List<CsaApplicationReviewReport> _reports = [];

    private CsaApplicationReview()
    {
    }

    public Guid Id { get; private set; }
    public Guid SchoolId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool ApplicationSuccessful { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTimeOffset SubmittedAtUtc { get; private set; }
    public CsaApplicationReviewStatus Status { get; private set; }
    public IReadOnlyList<CsaApplicationReviewReport> Reports => _reports;
    public bool IsVisible => Status is CsaApplicationReviewStatus.Visible or CsaApplicationReviewStatus.Approved;
    public int PostApprovalDistinctReportCount => _reports.Count(x => x.ResolvedAtUtc is null);

    public static CsaApplicationReview Submit(Guid schoolId, string name, bool applicationSuccessful, string comment, DateTimeOffset submittedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(schoolId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        return new CsaApplicationReview
        {
            Id = Guid.CreateVersion7(),
            SchoolId = schoolId,
            Name = name,
            ApplicationSuccessful = applicationSuccessful,
            Comment = comment,
            SubmittedAtUtc = submittedAtUtc,
            Status = CsaApplicationReviewStatus.Visible,
        };
    }

    public bool AttachReport(string reason, string? details, string? reporterFingerprint, DateTimeOffset submittedAtUtc)
    {
        if (!IsVisible)
        {
            throw new InvalidOperationException("Reports can only be attached to a visible review.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        reporterFingerprint = string.IsNullOrWhiteSpace(reporterFingerprint)
            ? null
            : reporterFingerprint;

        if (reporterFingerprint is not null && _reports.Exists(x => x.ResolvedAtUtc is null && string.Equals(x.ReporterFingerprint, reporterFingerprint, StringComparison.Ordinal)))
        {
            return false;
        }

        _reports.Add(new CsaApplicationReviewReport(Id, reason, details, reporterFingerprint, submittedAtUtc));

        if (Status == CsaApplicationReviewStatus.Visible)
        {
            HideAfterFirstReport();
        }
        else if (PostApprovalDistinctReportCount >= PostApprovalReportThreshold)
        {
            HideAfterReportThreshold();
        }

        return true;
    }

    public void Approve(DateTimeOffset resolvedAtUtc)
    {
        EnsurePendingModeration();
        ResolveOpenReports(resolvedAtUtc);
        Status = CsaApplicationReviewStatus.Approved;
    }

    public void Reject(DateTimeOffset resolvedAtUtc)
    {
        EnsurePendingModeration();
        ResolveOpenReports(resolvedAtUtc);
        Status = CsaApplicationReviewStatus.Rejected;
    }

    private void HideAfterFirstReport()
    {
        Status = CsaApplicationReviewStatus.PendingApproval;
    }

    private void HideAfterReportThreshold()
    {
        Status = CsaApplicationReviewStatus.PendingReapproval;
    }

    private void ResolveOpenReports(DateTimeOffset resolvedAtUtc)
    {
        foreach (var report in _reports.Where(x => x.ResolvedAtUtc is null))
        {
            report.Resolve(resolvedAtUtc);
        }
    }

    private void EnsurePendingModeration()
    {
        if (Status is not CsaApplicationReviewStatus.PendingApproval and not CsaApplicationReviewStatus.PendingReapproval)
        {
            throw new InvalidOperationException("Only a review pending moderation can be approved or rejected.");
        }
    }
}
