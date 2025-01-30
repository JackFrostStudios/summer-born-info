namespace SummerBornInfo.Web.Domain;

public class LocalAuthority
{
    public Guid Id { get; init; }
    public required int Code { get; init; }
    public required string Name { get; init; }
    public uint Version { get; init; }
}
