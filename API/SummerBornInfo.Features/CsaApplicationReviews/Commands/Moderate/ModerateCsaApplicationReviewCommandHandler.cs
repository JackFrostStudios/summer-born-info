namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.Moderate;

public sealed class ModerateCsaApplicationReviewCommandHandler
{
    public Task<ModerateCsaApplicationReviewResponse?> ExecuteAsync(
        ModerateCsaApplicationReviewCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var status = command.Decision?.Trim().ToLowerInvariant() switch
        {
            "approve" => ModerateCsaApplicationReviewResponse.ApprovedStatus,
            "reject" => ModerateCsaApplicationReviewResponse.RejectedStatus,
            _ => null,
        };

        if (status is null)
        {
            return Task.FromResult<ModerateCsaApplicationReviewResponse?>(null);
        }

        ModerateCsaApplicationReviewResponse response = new(
            command.ReviewId,
            status,
            DateTime.UtcNow,
            command.ModeratorNote);

        return Task.FromResult<ModerateCsaApplicationReviewResponse?>(response);
    }
}
