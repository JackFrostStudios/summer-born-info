namespace SummerBornInfo.Domain.Entities;

public class SchoolAddress
{
    public Guid SchoolId { get; set; }
    public string? Street { get; set; }
    public string? Locality { get; set; }
    public string? AddressThree { get; set; }
    public required string Town { get; set; }
    public string? County { get; set; }
    public required string PostCode { get; set; }
    public uint Version { get; set; }
}
