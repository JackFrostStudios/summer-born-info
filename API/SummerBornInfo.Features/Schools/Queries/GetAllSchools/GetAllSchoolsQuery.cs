namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools;

public sealed record GetAllSchoolsQuery(
    Guid? Cursor = null,
    int PageSize = 100
);
