namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public sealed record AdminCsaApplicationReviewQueueReportResponse(
    Guid Id,
    string Reason,
    string? Details,
    DateTimeOffset SubmittedAtUtc);
