﻿

namespace SummerBorn.Core.Entity;

public class School : IEntity
{
    public Guid Id { get; set; }
    public required int URN { get; set; }
    public required int UKPRN { get; set; }
    public required int EstablishmentNumber { get; set; }
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required DateOnly OpenDate { get; set; }
    public DateOnly CloseDate { get; set; }
    public required PhaseOfEducation PhaseOfEducation { get; set; }
    public required LocalAuthority LocalAuthority { get; set; }
    public required EstablishmentType EstablishmentType { get; set; }
    public required EstablishmentGroup EstablishmentGroup { get; set; }
    public required EstablishmentStatus EstablishmentStatus { get; set; }
    public uint Version { get; set; }
}
