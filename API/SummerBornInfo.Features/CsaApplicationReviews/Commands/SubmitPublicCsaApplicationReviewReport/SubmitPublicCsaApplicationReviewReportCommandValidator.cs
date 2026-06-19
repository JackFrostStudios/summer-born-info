namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReviewReport;

public static class SubmitPublicCsaApplicationReviewReportCommandValidator
{
    private static readonly HashSet<string> AllowedReasons = ["spam", "abusive", "privacy", "other"];

    public static bool TryValidate(
        Guid schoolId,
        Guid reviewId,
        string? reason,
        string? details,
        string? reporterFingerprint,
        out SubmitPublicCsaApplicationReviewReportCommand command,
        out IDictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        command = default!;

        var normalizedReason = reason?.Trim().ToLowerInvariant();
        var normalizedDetails = string.IsNullOrWhiteSpace(details)
            ? null
            : details.Trim();

        if (string.IsNullOrWhiteSpace(normalizedReason) || !AllowedReasons.Contains(normalizedReason))
        {
            errors["reason"] = ["Reason must be one of spam, abusive, privacy, or other."];
        }

        if (string.Equals(normalizedReason, "other", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(normalizedDetails))
        {
            errors["details"] = ["Details are required when reason is other."];
        }
        else if (normalizedDetails?.Length > 1000)
        {
            errors["details"] = ["Details must be 1000 characters or fewer."];
        }

        if (errors.Count > 0)
        {
            return false;
        }

        command = new SubmitPublicCsaApplicationReviewReportCommand(
            schoolId,
            reviewId,
            normalizedReason!,
            normalizedDetails,
            string.IsNullOrWhiteSpace(reporterFingerprint)
                ? null
                : reporterFingerprint.Trim());

        return true;
    }
}
