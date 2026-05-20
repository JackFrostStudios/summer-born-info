namespace SummerBornInfo.Domain.Entities;

public sealed class SchoolAddress
{
    public string? Street { get; set; }
    public string? Locality { get; set; }
    public string? AddressThree { get; set; }
    public required string Town { get; set; }
    public string? County { get; set; }
    public required string PostCode { get; set; }
}
