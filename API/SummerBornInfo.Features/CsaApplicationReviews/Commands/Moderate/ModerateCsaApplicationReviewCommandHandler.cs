namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.Moderate;

public sealed class ModerateCsaApplicationReviewCommandHandler(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ModerateCsaApplicationReviewExecutionResult> ExecuteAsync(
        ModerateCsaApplicationReviewCommand command,
        CancellationToken cancellationToken)
    {
        if (!TryParseDecision(command.Decision, out var decisionStatus))
        {
            return ModerateCsaApplicationReviewExecutionResult.InvalidDecision();
        }

        var review = await _context.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleOrDefaultAsync(x => x.Id == command.ReviewId, cancellationToken);

        if (review is null)
        {
            return ModerateCsaApplicationReviewExecutionResult.ReviewNotFound();
        }

        if (review.Status is not CsaApplicationReviewStatus.PendingApproval and not CsaApplicationReviewStatus.PendingReapproval)
        {
            return ModerateCsaApplicationReviewExecutionResult.ReviewNotPending();
        }

        var moderatedAtUtc = DateTimeOffset.UtcNow;
        if (string.Equals(decisionStatus, ModerateCsaApplicationReviewResponse.ApprovedStatus, StringComparison.Ordinal))
        {
            review.Approve(moderatedAtUtc);
        }
        else
        {
            review.Reject(moderatedAtUtc);
        }

        _ = await _context.SaveChangesAsync(cancellationToken);

        return ModerateCsaApplicationReviewExecutionResult.Succeeded(
            new ModerateCsaApplicationReviewResponse(
                review.Id,
                decisionStatus,
                moderatedAtUtc,
                command.ModeratorNote));
    }

    private static bool TryParseDecision(string? decision, out string normalizedDecision)
    {
        normalizedDecision = decision?.Trim().ToLowerInvariant() switch
        {
            "approve" => ModerateCsaApplicationReviewResponse.ApprovedStatus,
            "reject" => ModerateCsaApplicationReviewResponse.RejectedStatus,
            _ => string.Empty,
        };

        return !string.IsNullOrEmpty(normalizedDecision);
    }
}
