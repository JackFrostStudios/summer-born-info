namespace SummerBornInfo.Features.Schools.Queries.SearchSchools;

public sealed record SearchSchoolsQuery(
    string SearchText,
    int? PageSize = null)
{
    public const int DefaultPageSize = 100;
    public const int MaximumPageSize = 200;
}
