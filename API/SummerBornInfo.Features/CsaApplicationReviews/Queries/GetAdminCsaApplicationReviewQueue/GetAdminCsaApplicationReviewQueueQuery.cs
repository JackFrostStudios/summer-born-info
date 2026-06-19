namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public sealed record GetAdminCsaApplicationReviewQueueQuery(
    IReadOnlyList<CsaApplicationReviewStatus> QueueStates,
    string? Cursor,
    int? PageSize)
{
    public const int DefaultPageSize = 25;
    public const int MaximumPageSize = 200;
}
