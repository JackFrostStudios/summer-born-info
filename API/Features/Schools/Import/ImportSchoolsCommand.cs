using Domain.Entities;

namespace Features.Schools.Import;

public record ImportSchoolsCommand(Stream CsvStream, string FileName);

public record ImportSchoolsResponse(
    int TotalSchools,
    int SchoolsCreated,
    int SchoolsUpdated,
    int Errors
);