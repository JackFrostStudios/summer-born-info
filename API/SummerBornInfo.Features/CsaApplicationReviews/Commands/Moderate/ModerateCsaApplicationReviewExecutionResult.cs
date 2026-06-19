namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.Moderate;

public sealed record ModerateCsaApplicationReviewExecutionResult(
    ModerateCsaApplicationReviewExecutionStatus Status,
    ModerateCsaApplicationReviewResponse? Response)
{
    public static ModerateCsaApplicationReviewExecutionResult Succeeded(ModerateCsaApplicationReviewResponse response)
    {
        return new ModerateCsaApplicationReviewExecutionResult(
            ModerateCsaApplicationReviewExecutionStatus.Succeeded,
            response);
    }

    public static ModerateCsaApplicationReviewExecutionResult InvalidDecision()
    {
        return new ModerateCsaApplicationReviewExecutionResult(
            ModerateCsaApplicationReviewExecutionStatus.InvalidDecision,
            Response: null);
    }

    public static ModerateCsaApplicationReviewExecutionResult ReviewNotFound()
    {
        return new ModerateCsaApplicationReviewExecutionResult(
            ModerateCsaApplicationReviewExecutionStatus.ReviewNotFound,
            Response: null);
    }

    public static ModerateCsaApplicationReviewExecutionResult ReviewNotPending()
    {
        return new ModerateCsaApplicationReviewExecutionResult(
            ModerateCsaApplicationReviewExecutionStatus.ReviewNotPending,
            Response: null);
    }
}
