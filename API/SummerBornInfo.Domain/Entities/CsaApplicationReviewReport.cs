namespace SummerBornInfo.Domain.Entities;

public sealed class CsaApplicationReviewReport
{
    private CsaApplicationReviewReport()
    {
    }

    internal CsaApplicationReviewReport(Guid reviewId, string reason, string? details, string? reporterFingerprint, DateTimeOffset submittedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(reviewId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Id = Guid.CreateVersion7();
        CsaApplicationReviewId = reviewId;
        Reason = reason;
        Details = details;
        ReporterFingerprint = reporterFingerprint;
        SubmittedAtUtc = submittedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid CsaApplicationReviewId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public string? ReporterFingerprint { get; private set; }
    public DateTimeOffset SubmittedAtUtc { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }

    internal void Resolve(DateTimeOffset resolvedAtUtc)
    {
        ResolvedAtUtc ??= resolvedAtUtc;
    }
}
