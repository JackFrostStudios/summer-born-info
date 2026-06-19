namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReviewReport;

public sealed record SubmitPublicCsaApplicationReviewReportExecutionResult(
    SubmitPublicCsaApplicationReviewReportExecutionStatus Status,
    SubmitPublicCsaApplicationReviewReportResponse? Response)
{
    public static SubmitPublicCsaApplicationReviewReportExecutionResult Accepted(SubmitPublicCsaApplicationReviewReportResponse response)
    {
        return new SubmitPublicCsaApplicationReviewReportExecutionResult(
            SubmitPublicCsaApplicationReviewReportExecutionStatus.Accepted,
            response);
    }

    public static SubmitPublicCsaApplicationReviewReportExecutionResult SchoolNotFound()
    {
        return new SubmitPublicCsaApplicationReviewReportExecutionResult(
            SubmitPublicCsaApplicationReviewReportExecutionStatus.SchoolNotFound,
            Response: null);
    }

    public static SubmitPublicCsaApplicationReviewReportExecutionResult ReviewNotFound()
    {
        return new SubmitPublicCsaApplicationReviewReportExecutionResult(
            SubmitPublicCsaApplicationReviewReportExecutionStatus.ReviewNotFound,
            Response: null);
    }

    public static SubmitPublicCsaApplicationReviewReportExecutionResult BotVerificationFailed()
    {
        return new SubmitPublicCsaApplicationReviewReportExecutionResult(
            SubmitPublicCsaApplicationReviewReportExecutionStatus.BotVerificationFailed,
            Response: null);
    }
}
