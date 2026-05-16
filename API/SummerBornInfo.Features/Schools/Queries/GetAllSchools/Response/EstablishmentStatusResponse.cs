namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools.Response;

public sealed record EstablishmentStatusResponse
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static EstablishmentStatusResponse FromEntity(EstablishmentStatus establishmentStatus) =>
        new()
        {
            Id = establishmentStatus.Id,
            Code = establishmentStatus.Code,
            Name = establishmentStatus.Name,
        };
}
