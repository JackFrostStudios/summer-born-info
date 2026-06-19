namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public sealed record GetAdminCsaApplicationReviewQueueResponse(
    IReadOnlyList<GetAdminCsaApplicationReviewQueueItemResponse> Reviews,
    string? NextCursor);
