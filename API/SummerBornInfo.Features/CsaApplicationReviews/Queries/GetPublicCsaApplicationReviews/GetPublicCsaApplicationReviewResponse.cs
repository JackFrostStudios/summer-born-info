namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetPublicCsaApplicationReviews;

public sealed record GetPublicCsaApplicationReviewResponse(
    Guid Id,
    string Name,
    bool ApplicationSuccessful,
    string Comment,
    DateTimeOffset SubmittedAtUtc);
