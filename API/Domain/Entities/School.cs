namespace SummerBornInfo.Domain.Entities;

public sealed class School
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required int URN { get; set; }
    public int? UKPRN { get; set; }
    public required int EstablishmentNumber { get; set; }
    public required string Name { get; set; }
    public required SchoolAddress Address { get; set; }
    public DateOnly? OpenDate { get; set; }
    public DateOnly? CloseDate { get; set; }
    public required PhaseOfEducation PhaseOfEducation { get; set; }
    public required LocalAuthority LocalAuthority { get; set; }
    public required EstablishmentType EstablishmentType { get; set; }
    public required EstablishmentGroup EstablishmentGroup { get; set; }
    public required EstablishmentStatus EstablishmentStatus { get; set; }
}
