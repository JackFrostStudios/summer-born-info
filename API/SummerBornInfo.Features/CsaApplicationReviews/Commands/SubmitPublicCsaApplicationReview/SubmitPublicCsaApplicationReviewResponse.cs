namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReview;

public sealed record SubmitPublicCsaApplicationReviewResponse(
    Guid Id,
    Guid SchoolId,
    string Name,
    bool ApplicationSuccessful,
    string Comment,
    string Status,
    DateTimeOffset SubmittedAtUtc)
{
    public static SubmitPublicCsaApplicationReviewResponse FromEntity(CsaApplicationReview review)
    {
        return new SubmitPublicCsaApplicationReviewResponse(
            review.Id,
            review.SchoolId,
            review.Name,
            review.ApplicationSuccessful,
            review.Comment,
            review.Status.ToString().ToLowerInvariant(),
            review.SubmittedAtUtc);
    }
}
