namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetPublicCsaApplicationReviews;

public sealed record GetPublicCsaApplicationReviewsQuery(
    Guid SchoolId,
    string? Cursor,
    int? PageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 200;
}
