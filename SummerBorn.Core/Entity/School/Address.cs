namespace SummerBorn.Core.Entity.School;

public class Address
{
    public Guid SchoolId { get; set; }
    public string? Street { get; set; }
    public string? Locality { get; set; }
    public string? AddressThree { get; set; }
    public string? Town { get; set; }
    public string? County { get; set; }
    public string? PostCode { get; set; }
    public uint Version { get; set; }
}
