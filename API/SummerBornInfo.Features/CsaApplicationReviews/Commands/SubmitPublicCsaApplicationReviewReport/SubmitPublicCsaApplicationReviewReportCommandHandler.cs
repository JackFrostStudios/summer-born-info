namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReviewReport;

public sealed class SubmitPublicCsaApplicationReviewReportCommandHandler(
    ApplicationDbContext context,
    IAnonymousBotVerifier botVerifier)
{
    private readonly ApplicationDbContext _context = context;
    private readonly IAnonymousBotVerifier _botVerifier = botVerifier;

    public async Task<SubmitPublicCsaApplicationReviewReportExecutionResult> ExecuteAsync(
        SubmitPublicCsaApplicationReviewReportCommand command,
        CancellationToken cancellationToken)
    {
        var botVerification = await _botVerifier.VerifyAsync(
            new AnonymousBotVerificationRequest(
                command.BotVerificationToken,
                command.RemoteIpAddress),
            cancellationToken);

        if (!botVerification.IsVerified)
        {
            return SubmitPublicCsaApplicationReviewReportExecutionResult.BotVerificationFailed();
        }

        var schoolExists = await _context.Schools
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.SchoolId, cancellationToken);

        if (!schoolExists)
        {
            return SubmitPublicCsaApplicationReviewReportExecutionResult.SchoolNotFound();
        }

        var review = await _context.CsaApplicationReviews
            .Include(x => x.Reports)
            .SingleOrDefaultAsync(
                x => x.Id == command.ReviewId
                    && x.SchoolId == command.SchoolId
                    && (x.Status == CsaApplicationReviewStatus.Visible || x.Status == CsaApplicationReviewStatus.Approved),
                cancellationToken);

        if (review is null)
        {
            return SubmitPublicCsaApplicationReviewReportExecutionResult.ReviewNotFound();
        }

        var reportedAtUtc = DateTimeOffset.UtcNow;
        var reportAttached = review.AttachReport(
            command.Reason,
            command.Details,
            command.ReporterFingerprint,
            reportedAtUtc);

        if (reportAttached)
        {
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        return SubmitPublicCsaApplicationReviewReportExecutionResult.Accepted(
            new SubmitPublicCsaApplicationReviewReportResponse(
                review.Id,
                "reportAccepted",
                reportedAtUtc));
    }
}
