namespace SummerBorn.Core.Entity.Establishment;

public class EstablishmentType : IEntity
{
    public Guid Id { get; set; }
    public required int Code { get; set; }
    public required string Name { get; set; }
    public uint Version { get; set; }
}
