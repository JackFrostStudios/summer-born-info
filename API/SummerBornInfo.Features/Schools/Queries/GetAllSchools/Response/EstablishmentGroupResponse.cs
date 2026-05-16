namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools.Response;

public sealed record EstablishmentGroupResponse
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static EstablishmentGroupResponse FromEntity(EstablishmentGroup establishmentGroup) =>
        new()
        {
            Id = establishmentGroup.Id,
            Code = establishmentGroup.Code,
            Name = establishmentGroup.Name,
        };
}
