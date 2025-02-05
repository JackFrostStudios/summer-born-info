namespace SummerBornInfo.Web.Domain;

public class School
{
    public Guid Id { get; init; }
    public required int URN { get; init; }
    public required int UKPRN { get; init; }
    public required int EstablishmentNumber { get; init; }
    public required string Name { get; set; }
    public required SchoolAddress Address { get; set; }
    public required DateOnly OpenDate { get; set; }
    public DateOnly CloseDate { get; set; }
    public required PhaseOfEducation PhaseOfEducation { get; set; }
    public required LocalAuthority LocalAuthority { get; set; }
    public required EstablishmentType EstablishmentType { get; set; }
    public required EstablishmentGroup EstablishmentGroup { get; set; }
    public required EstablishmentStatus EstablishmentStatus { get; set; }
    public uint Version { get; init; }
}
