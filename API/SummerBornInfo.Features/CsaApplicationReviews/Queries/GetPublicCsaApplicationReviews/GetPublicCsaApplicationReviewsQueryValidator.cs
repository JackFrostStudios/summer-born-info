namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetPublicCsaApplicationReviews;

public static class GetPublicCsaApplicationReviewsQueryValidator
{
    public static bool TryValidate(Guid schoolId, string? cursor, int? pageSize, out GetPublicCsaApplicationReviewsQuery query)
    {
        query = default!;

        if (pageSize is <= 0 or > GetPublicCsaApplicationReviewsQuery.MaximumPageSize)
        {
            return false;
        }

        var normalizedCursor = cursor?.Trim();
        if (normalizedCursor is not null)
        {
            var effectivePageSize = pageSize ?? GetPublicCsaApplicationReviewsQuery.DefaultPageSize;

            if (string.IsNullOrWhiteSpace(normalizedCursor)
                || !PublicCsaApplicationReviewsCursor.TryDecode(normalizedCursor, out var decodedCursor)
                || decodedCursor.PageSize != effectivePageSize)
            {
                return false;
            }
        }

        query = new GetPublicCsaApplicationReviewsQuery(schoolId, normalizedCursor, pageSize);
        return true;
    }
}
