namespace SummerBornInfo.Web.Domain;

public class SchoolAddress
{
    public Guid SchoolId { get; set; }
    public string? Street { get; init; }
    public string? Locality { get; init; }
    public string? AddressThree { get; init; }
    public string? Town { get; init; }
    public string? County { get; init; }
    public string? PostCode { get; init; }
    public uint Version { get; init; }
}
