namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReviewReport;

public sealed record SubmitPublicCsaApplicationReviewReportCommand(
    Guid SchoolId,
    Guid ReviewId,
    string Reason,
    string? Details,
    string? ReporterFingerprint,
    string? BotVerificationToken,
    string? RemoteIpAddress);
