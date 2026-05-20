namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools;

public sealed record GetAllSchoolsQuery(
    Guid? Cursor = null,
    int? PageSize = null
)
{
    public const int DefaultPageSize = 100;
    public const int MaximumPageSize = 200;
}
