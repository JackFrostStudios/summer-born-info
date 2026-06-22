namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReview;

public sealed class SubmitPublicCsaApplicationReviewCommandHandler(
    ApplicationDbContext context,
    IAnonymousBotVerifier botVerifier)
{
    private readonly ApplicationDbContext _context = context;
    private readonly IAnonymousBotVerifier _botVerifier = botVerifier;

    public async Task<SubmitPublicCsaApplicationReviewExecutionResult> ExecuteAsync(
        SubmitPublicCsaApplicationReviewCommand command,
        CancellationToken cancellationToken)
    {
        var botVerification = await _botVerifier.VerifyAsync(
            new AnonymousBotVerificationRequest(
                command.BotVerificationToken,
                command.RemoteIpAddress),
            cancellationToken);

        if (!botVerification.IsVerified)
        {
            return SubmitPublicCsaApplicationReviewExecutionResult.BotVerificationFailed();
        }

        var schoolExists = await _context.Schools
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.SchoolId, cancellationToken);

        if (!schoolExists)
        {
            return SubmitPublicCsaApplicationReviewExecutionResult.SchoolNotFound();
        }

        var review = CsaApplicationReview.Submit(
            command.SchoolId,
            command.Name,
            command.ApplicationSuccessful,
            command.Comment,
            DateTimeOffset.UtcNow);

        _ = _context.CsaApplicationReviews.Add(review);
        _ = await _context.SaveChangesAsync(cancellationToken);

        return SubmitPublicCsaApplicationReviewExecutionResult.Created(
            SubmitPublicCsaApplicationReviewResponse.FromEntity(review));
    }
}
