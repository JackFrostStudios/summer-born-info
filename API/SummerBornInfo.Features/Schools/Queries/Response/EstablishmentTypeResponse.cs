namespace SummerBornInfo.Features.Schools.Queries.Response;

public sealed record EstablishmentTypeResponse
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static EstablishmentTypeResponse FromEntity(EstablishmentType establishmentType)
    {
        return new()
        {
            Id = establishmentType.Id,
            Code = establishmentType.Code,
            Name = establishmentType.Name,
        };
    }
}
