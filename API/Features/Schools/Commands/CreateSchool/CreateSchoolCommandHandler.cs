using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Features.Schools.Commands.CreateSchool;

public class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, CreateSchoolResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateSchoolCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateSchoolResponse> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
    {
        // Check if URN already exists
        var existingSchool = await _context.Schools
            .AnyAsync(s => s.URN == request.URN, cancellationToken);
        
        if (existingSchool)
        {
            throw new InvalidOperationException($"A school with URN '{request.URN}' already exists.");
        }

        var school = new School
        {
            Name = request.Name,
            URN = request.URN,
            Address = request.Address,
            City = request.City,
            County = request.County,
            Postcode = request.Postcode,
            PhoneNumber = request.PhoneNumber,
            Website = request.Website,
            Type = request.Type,
            Capacity = request.Capacity,
            PupilsEnrolled = request.PupilsEnrolled
        };

        _context.Schools.Add(school);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateSchoolResponse(school.Id);
    }
}