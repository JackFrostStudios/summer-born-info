namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools;

public sealed class GetAllSchoolsQueryHandler(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<(List<SchoolResponse> Schools, Guid? NextCursor)> ExecuteAsync(GetAllSchoolsQuery request, CancellationToken cancellationToken)
    {
        var requestedPageSize = request.PageSize ?? GetAllSchoolsQuery.DefaultPageSize;
        var pageSize = Math.Min(requestedPageSize, GetAllSchoolsQuery.MaximumPageSize);

        var query = _context.Schools
            .AsNoTracking();

        if (request.Cursor.HasValue)
        {
            query = query.Where(s => s.Id > request.Cursor.Value);
        }

        query = query.OrderBy(s => s.Id);

        // Take one extra to determine if there's a next page
        var schools = await query
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var hasMore = schools.Count > pageSize;
        var schoolsToReturn = hasMore ? [.. schools.Take(pageSize)] : schools;

        var schoolResponses = schoolsToReturn.Select(SchoolResponse.FromEntity).ToList();

        Guid? nextCursor = null;
        if (hasMore)
        {
            nextCursor = schoolsToReturn[^1].Id;
        }

        return (schoolResponses, nextCursor);
    }
}
