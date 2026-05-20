namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools.Response;

public sealed record LocalAuthorityResponse
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static LocalAuthorityResponse FromEntity(LocalAuthority localAuthority)
    {
        return new()
        {
            Id = localAuthority.Id,
            Code = localAuthority.Code,
            Name = localAuthority.Name,
        };
    }
}
