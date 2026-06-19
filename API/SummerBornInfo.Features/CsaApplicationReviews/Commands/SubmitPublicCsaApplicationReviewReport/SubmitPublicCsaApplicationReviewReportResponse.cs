namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReviewReport;

public sealed record SubmitPublicCsaApplicationReviewReportResponse(
    Guid Id,
    string Status,
    DateTimeOffset ReportedAtUtc);
