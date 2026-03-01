namespace Domain.Entities;

public class School
{
    public Guid Id { get; set; }
    public required int URN { get; set; }
    public int? UKPRN { get; set; }
    public required int EstablishmentNumber { get; set; }
    public required string Name { get; set; }
    public required SchoolAddress Address { get; set; }
    public DateOnly? OpenDate { get; set; }
    public DateOnly? CloseDate { get; set; }
    public required Guid PhaseOfEducationId { get; set; }
    public required PhaseOfEducation PhaseOfEducation { get; set; }
    public required Guid LocalAuthorityId { get; set; }
    public required LocalAuthority LocalAuthority { get; set; }
    public required Guid EstablishmentTypeId { get; set; }
    public required EstablishmentType EstablishmentType { get; set; }
    public required Guid EstablishmentGroupId { get; set; }
    public required EstablishmentGroup EstablishmentGroup { get; set; }
    public required Guid EstablishmentStatusId { get; set; }
    public required EstablishmentStatus EstablishmentStatus { get; set; }
    public uint Version { get; set; }
}
