namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.Moderate;

public sealed record ModerateCsaApplicationReviewResponse(
    Guid Id,
    string Status,
    DateTimeOffset ModeratedAtUtc,
    string? ModeratorNote)
{
    public const string ApprovedStatus = "approved";
    public const string RejectedStatus = "rejected";
}
