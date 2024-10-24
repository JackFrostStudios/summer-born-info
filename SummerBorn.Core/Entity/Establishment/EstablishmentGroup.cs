namespace SummerBorn.Core.Entity.Establishment;

public class EstablishmentGroup : IEntity
{
    public Guid Id { get; set; }
    public required int Code { get; set; }
    public required string Name { get; set; }
    public uint Version { get; set; }
}
