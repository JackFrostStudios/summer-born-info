namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools.Response;

public sealed record SchoolAddressResponse
{
    public required string? Street { get; init; }
    public required string? Locality { get; init; }
    public required string? AddressThree { get; init; }
    public required string Town { get; init; }
    public required string? County { get; init; }
    public required string PostCode { get; init; }
    public static SchoolAddressResponse FromEntity(SchoolAddress schoolAddress) => new()
    {
        Street = schoolAddress.Street,
        Locality = schoolAddress.Locality,
        AddressThree = schoolAddress.AddressThree,
        Town = schoolAddress.Town,
        County = schoolAddress.County,
        PostCode = schoolAddress.PostCode,
    };

};
