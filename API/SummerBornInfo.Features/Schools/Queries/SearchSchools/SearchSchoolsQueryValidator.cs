namespace SummerBornInfo.Features.Schools.Queries.SearchSchools;

public static class SearchSchoolsQueryValidator
{
    public static bool TryValidate(string? q, int? pageSize, out SearchSchoolsQuery query)
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

        query = new SearchSchoolsQuery(searchText, pageSize);
        return true;
    }
}
