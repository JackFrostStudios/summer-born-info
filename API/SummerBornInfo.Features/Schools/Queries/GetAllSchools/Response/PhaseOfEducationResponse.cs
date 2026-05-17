namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools.Response;

public sealed record PhaseOfEducationResponse
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static PhaseOfEducationResponse FromEntity(PhaseOfEducation phaseOfEducation)
    {
        return new()
        {
            Id = phaseOfEducation.Id,
            Code = phaseOfEducation.Code,
            Name = phaseOfEducation.Name,
        };
    }
}
