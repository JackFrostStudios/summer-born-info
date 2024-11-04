namespace SummerBorn.Core.Commands;
public record class CreateBasicLookupEntity
{
    public required int Code { get; init; }
    public required string Name { get; init; }
}
