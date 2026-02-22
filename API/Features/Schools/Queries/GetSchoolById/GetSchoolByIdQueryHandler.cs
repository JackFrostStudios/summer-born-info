using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Features.Schools.Queries.GetSchoolById;

public class GetSchoolByIdQueryHandler : IRequestHandler<GetSchoolByIdQuery, SchoolDto?>
{
    private readonly ApplicationDbContext _context;

    public GetSchoolByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolDto?> Handle(GetSchoolByIdQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (school == null)
        {
            return null;
        }

        return new SchoolDto(
            school.Id,
            school.Name,
            school.URN,
            school.Address,
            school.City,
            school.County,
            school.Postcode,
            school.PhoneNumber,
            school.Website,
            school.Type,
            school.Capacity,
            school.PupilsEnrolled
        );
    }
}