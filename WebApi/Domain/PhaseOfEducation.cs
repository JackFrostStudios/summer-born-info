namespace SummerBornInfo.Domain;

public class PhaseOfEducation
{
    public Guid Id { get; set; }
    public required int Code { get; set; }
    public required string Name { get; set; }
    public uint Version { get; set; }
}
