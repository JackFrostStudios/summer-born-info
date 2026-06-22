namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public sealed record GetAdminCsaApplicationReviewQueueItemResponse(
    Guid Id,
    string ReviewerName,
    bool ApplicationSuccessful,
    string Comment,
    string Status,
    DateTimeOffset SubmittedAtUtc,
    int OpenReportCount,
    int PostApprovalDistinctReportCount,
    DateTimeOffset LatestReportAtUtc,
    AdminCsaApplicationReviewQueueSchoolResponse School,
    IReadOnlyList<AdminCsaApplicationReviewQueueReportResponse> Reports);
