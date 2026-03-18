namespace SummerBornInfo.Domain.Entities;

public sealed class LocalAuthority
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Code { get; set; }
    public required string Name { get; set; }
    public uint Version { get; set; }
}
