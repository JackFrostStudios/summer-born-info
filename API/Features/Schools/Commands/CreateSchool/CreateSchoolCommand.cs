using Domain.Entities;
using MediatR;

namespace Features.Schools.Commands.CreateSchool;

public record CreateSchoolCommand(
    string Name,
    string URN,
    string? Address,
    string? City,
    string? County,
    string? Postcode,
    string? PhoneNumber,
    string? Website,
    SchoolType Type,
    int? Capacity,
    int? PupilsEnrolled
) : IRequest<CreateSchoolResponse>;

public record CreateSchoolResponse(int Id);