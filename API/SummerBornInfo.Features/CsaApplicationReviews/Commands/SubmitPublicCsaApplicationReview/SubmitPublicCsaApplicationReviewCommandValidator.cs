namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReview;

public static class SubmitPublicCsaApplicationReviewCommandValidator
{
    public static bool TryValidate(
        Guid schoolId,
        string? name,
        bool? applicationSuccessful,
        string? comment,
        string? botVerificationToken,
        string? remoteIpAddress,
        out SubmitPublicCsaApplicationReviewCommand command,
        out IDictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        command = default!;

        var trimmedName = name?.Trim();
        var trimmedComment = comment?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            errors["name"] = ["Name is required."];
        }
        else if (trimmedName.Length > 200)
        {
            errors["name"] = ["Name must be 200 characters or fewer."];
        }

        if (!applicationSuccessful.HasValue)
        {
            errors["applicationSuccessful"] = ["Application successful is required."];
        }

        if (string.IsNullOrWhiteSpace(trimmedComment))
        {
            errors["comment"] = ["Comment is required."];
        }
        else if (trimmedComment.Length > 4000)
        {
            errors["comment"] = ["Comment must be 4000 characters or fewer."];
        }

        if (errors.Count > 0)
        {
            return false;
        }

        command = new SubmitPublicCsaApplicationReviewCommand(
            schoolId,
            trimmedName!,
            applicationSuccessful!.Value,
            trimmedComment!,
            string.IsNullOrWhiteSpace(botVerificationToken)
                ? null
                : botVerificationToken.Trim(),
            string.IsNullOrWhiteSpace(remoteIpAddress)
                ? null
                : remoteIpAddress.Trim());

        return true;
    }
}
