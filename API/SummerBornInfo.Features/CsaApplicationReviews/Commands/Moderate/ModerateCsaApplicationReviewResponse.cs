namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.Moderate;

public sealed record ModerateCsaApplicationReviewResponse(
    string Id,
    string Status,
    DateTime ModeratedAtUtc,
    string? ModeratorNote)
{
    public const string ApprovedStatus = "approved";
    public const string RejectedStatus = "rejected";
}
