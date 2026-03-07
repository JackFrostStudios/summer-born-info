namespace SummerBornInfo.Domain.Entities;

public class EstablishmentGroup
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public uint Version { get; set; }
}
