namespace SummerBornInfo.Features.Schools.Queries.GetSchoolByUrn;

public sealed class GetSchoolByUrnQueryHandler(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<SchoolResponse?> ExecuteAsync(GetSchoolByUrnQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .AsNoTracking()
            .SingleOrDefaultAsync(school => school.URN == request.Urn, cancellationToken);

        return school is null ? null : SchoolResponse.FromEntity(school);
    }
}
