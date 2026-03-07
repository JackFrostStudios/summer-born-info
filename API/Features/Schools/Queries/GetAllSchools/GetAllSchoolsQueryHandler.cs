namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools;

public class GetAllSchoolsQueryHandler
{
    private readonly ApplicationDbContext _context;

    public GetAllSchoolsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<SchoolDto> Schools, Guid? NextCursor)> ExecuteAsync(GetAllSchoolsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<School> query = _context.Schools
            .AsNoTracking()
            .Include(s => s.Address)
            .Include(s => s.PhaseOfEducation)
            .Include(s => s.LocalAuthority)
            .Include(s => s.EstablishmentType)
            .Include(s => s.EstablishmentGroup)
            .Include(s => s.EstablishmentStatus);

        // Apply cursor-based pagination
        if (request.Cursor.HasValue)
        {
            query = query.Where(s => s.Id > request.Cursor.Value);
        }

        query = query.OrderBy(s => s.Id);

        // Take one extra to determine if there's a next page
        var schools = await query
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken);

        var hasMore = schools.Count > request.PageSize;
        var schoolsToReturn = hasMore ? schools.Take(request.PageSize).ToList() : schools;

        var schoolDtos = schoolsToReturn.Select(s => new SchoolDto(
            s.Id,
            s.URN,
            s.UKPRN,
            s.EstablishmentNumber,
            s.Name,
            new SchoolAddressDto(
                s.Address.SchoolId,
                s.Address.Street,
                s.Address.Locality,
                s.Address.AddressThree,
                s.Address.Town,
                s.Address.County,
                s.Address.PostCode
            ),
            s.OpenDate,
            s.CloseDate,
            new PhaseOfEducationDto(s.PhaseOfEducation.Id, s.PhaseOfEducation.Code, s.PhaseOfEducation.Name),
            new LocalAuthorityDto(s.LocalAuthority.Id, s.LocalAuthority.Code, s.LocalAuthority.Name),
            new EstablishmentTypeDto(s.EstablishmentType.Id, s.EstablishmentType.Code, s.EstablishmentType.Name),
            new EstablishmentGroupDto(s.EstablishmentGroup.Id, s.EstablishmentGroup.Code, s.EstablishmentGroup.Name),
            new EstablishmentStatusDto(s.EstablishmentStatus.Id, s.EstablishmentStatus.Code, s.EstablishmentStatus.Name)
        )).ToList();

        Guid? nextCursor = null;
        if (hasMore)
        {
            nextCursor = schoolsToReturn.Last().Id;
        }

        return (schoolDtos, nextCursor);
    }
}