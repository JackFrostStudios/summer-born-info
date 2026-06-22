namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.Moderate;

public sealed record ModerateCsaApplicationReviewCommand(
    Guid ReviewId,
    string? Decision,
    string? ModeratorNote);
