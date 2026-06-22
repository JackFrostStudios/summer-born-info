namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetPublicCsaApplicationReviews;

public sealed record GetPublicCsaApplicationReviewsResponse(
    IReadOnlyList<GetPublicCsaApplicationReviewResponse> Reviews,
    string? NextCursor);
