namespace SummerBornInfo.Web.Domain.School;

public class School
{
    public Guid Id { get; init; }
    public required int URN { get; init; }
    public required int UKPRN { get; init; }
    public required int EstablishmentNumber { get; init; }
    public required string Name { get; init; }
    public required Address Address { get; init; }
    public required DateOnly OpenDate { get; init; }
    public DateOnly CloseDate { get; init; }
    public required PhaseOfEducation PhaseOfEducation { get; init; }
    public required LocalAuthority LocalAuthority { get; init; }
    public required EstablishmentType EstablishmentType { get; init; }
    public required EstablishmentGroup EstablishmentGroup { get; init; }
    public required EstablishmentStatus EstablishmentStatus { get; init; }
    public uint Version { get; init; }
}
