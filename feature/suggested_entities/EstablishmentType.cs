namespace SummerBornInfo.Web.Domain;

public class EstablishmentType
{
    public Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; set; }
    public uint Version { get; init; }
}
