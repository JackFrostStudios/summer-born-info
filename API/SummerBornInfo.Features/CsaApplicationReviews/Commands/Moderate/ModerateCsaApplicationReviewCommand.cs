namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.Moderate;

public sealed record ModerateCsaApplicationReviewCommand(
    string ReviewId,
    string? Decision,
    string? ModeratorNote);
