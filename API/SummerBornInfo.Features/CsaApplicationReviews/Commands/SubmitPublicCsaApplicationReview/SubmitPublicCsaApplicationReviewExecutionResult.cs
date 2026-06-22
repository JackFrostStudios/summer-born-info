namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReview;

public sealed record SubmitPublicCsaApplicationReviewExecutionResult(
    SubmitPublicCsaApplicationReviewExecutionStatus Status,
    SubmitPublicCsaApplicationReviewResponse? Response)
{
    public static SubmitPublicCsaApplicationReviewExecutionResult Created(SubmitPublicCsaApplicationReviewResponse response)
    {
        return new SubmitPublicCsaApplicationReviewExecutionResult(
            SubmitPublicCsaApplicationReviewExecutionStatus.Created,
            response);
    }

    public static SubmitPublicCsaApplicationReviewExecutionResult SchoolNotFound()
    {
        return new SubmitPublicCsaApplicationReviewExecutionResult(
            SubmitPublicCsaApplicationReviewExecutionStatus.SchoolNotFound,
            Response: null);
    }

    public static SubmitPublicCsaApplicationReviewExecutionResult BotVerificationFailed()
    {
        return new SubmitPublicCsaApplicationReviewExecutionResult(
            SubmitPublicCsaApplicationReviewExecutionStatus.BotVerificationFailed,
            Response: null);
    }
}
