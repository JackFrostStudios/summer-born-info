namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReview;

public sealed record SubmitPublicCsaApplicationReviewCommand(
    Guid SchoolId,
    string Name,
    bool ApplicationSuccessful,
    string Comment,
    string? BotVerificationToken,
    string? RemoteIpAddress);
