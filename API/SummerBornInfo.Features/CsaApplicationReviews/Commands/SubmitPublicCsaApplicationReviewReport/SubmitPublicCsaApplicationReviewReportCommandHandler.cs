namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReviewReport;

public sealed class SubmitPublicCsaApplicationReviewReportCommandHandler(
    ApplicationDbContext context,
    IAnonymousBotVerifier botVerifier)
{
    private const string DuplicateOpenFingerprintConstraintName = "ux_csa_application_review_report_open_fingerprint";

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
            try
            {
                _ = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsDuplicateOpenFingerprintViolation(exception))
            {
                // A concurrent duplicate report from the same fingerprint should be accepted as a no-op.
            }
        }

        return SubmitPublicCsaApplicationReviewReportExecutionResult.Accepted(
            new SubmitPublicCsaApplicationReviewReportResponse(
                review.Id,
                "reportAccepted",
                reportedAtUtc));
    }

    private static bool IsDuplicateOpenFingerprintViolation(DbUpdateException exception)
    {
        if (exception.InnerException is not PostgresException postgresException)
        {
            return false;
        }

        return string.Equals(postgresException.SqlState, PostgresErrorCodes.UniqueViolation, StringComparison.Ordinal)
            && string.Equals(postgresException.ConstraintName, DuplicateOpenFingerprintConstraintName, StringComparison.Ordinal);
    }
}
