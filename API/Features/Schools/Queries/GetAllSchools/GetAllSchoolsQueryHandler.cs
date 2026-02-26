using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Features.Schools.Queries.GetAllSchools;

public class GetAllSchoolsQueryHandler
{
    private readonly ApplicationDbContext _context;

    public GetAllSchoolsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SchoolDto>> ExecuteAsync(GetAllSchoolsQuery request, CancellationToken cancellationToken)
    {
        var schools = await _context.Schools
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return schools.Select(s => new SchoolDto(
            s.Id,
            s.Name,
            s.URN,
            s.Address,
            s.City,
            s.County,
            s.Postcode,
            s.PhoneNumber,
            s.Website,
            s.Type,
            s.Capacity,
            s.PupilsEnrolled
        )).ToList();
    }
}
