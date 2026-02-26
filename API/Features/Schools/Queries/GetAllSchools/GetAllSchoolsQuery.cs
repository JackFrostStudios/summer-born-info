using Domain.Entities;

namespace Features.Schools.Queries.GetAllSchools;

public record GetAllSchoolsQuery();

public record SchoolDto(
    int Id,
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
);
