namespace SummerBornInfo.Features.Schools.Queries.SearchSchools;

public static class SearchSchoolsQueryValidator
{
    public static bool TryValidate(string? q, string? cursor, int? pageSize, out SearchSchoolsQuery query)
    {
        query = default!;

        if (string.IsNullOrWhiteSpace(q))
        {
            return false;
        }

        var searchText = q.Trim();
        if (searchText.Length < 4)
        {
            return false;
        }

        if (pageSize is <= 0 or > SearchSchoolsQuery.MaximumPageSize)
        {
            return false;
        }

        var normalizedQuery = searchText.ToLowerInvariant();
        if (cursor is not null
            && !SearchSchoolsCursor.TryDecode(cursor, normalizedQuery, out _))
        {
            return false;
        }

        query = new SearchSchoolsQuery(searchText, cursor, pageSize);
        return true;
    }
}
