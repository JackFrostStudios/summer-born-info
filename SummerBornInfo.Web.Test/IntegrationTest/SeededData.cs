﻿namespace SummerBornInfo.Web.Test.IntegrationTest;
public class SeededData
{
    public PhaseOfEducation PhaseOfEducation { get; private init; }
    public LocalAuthority LocalAuthority { get; private init; }
    public EstablishmentGroup EstablishmentGroup { get; private init; }
    public EstablishmentStatus EstablishmentStatus { get; private init; }
    public EstablishmentType EstablishmentType { get; private init; }
    public School School { get; private init; }
    private readonly SchoolContext schoolContext;

    public SeededData(SchoolContext SchoolContext)
    {
        schoolContext = SchoolContext;

        PhaseOfEducation = new PhaseOfEducation { Code = 10000, Name = "Primary" };
        LocalAuthority = new LocalAuthority { Code = 20000, Name = "Authority" };
        EstablishmentGroup = new EstablishmentGroup { Code = 30000, Name = "Establishment Group" };
        EstablishmentStatus = new EstablishmentStatus { Code = 40000, Name = "Open" };
        EstablishmentType = new EstablishmentType { Code = 50000, Name = "Community School" };
        var address = new Address { Street = "Str", Locality = "loc", AddressThree = "addr3", Town = "twn", County = "Coun", PostCode = "PT12CD" };

        schoolContext.Add(PhaseOfEducation);
        schoolContext.Add(LocalAuthority);
        schoolContext.Add(EstablishmentGroup);
        schoolContext.Add(EstablishmentStatus);
        schoolContext.Add(EstablishmentType);
        schoolContext.SaveChanges();

        School = new School
        {
            Address = address,
            CloseDate = new DateOnly(2024, 10, 21),
            EstablishmentGroup = EstablishmentGroup,
            EstablishmentNumber = 60000,
            EstablishmentStatus = EstablishmentStatus,
            EstablishmentType = EstablishmentType,
            LocalAuthority = LocalAuthority,
            Name = "Local Primary School",
            OpenDate = new DateOnly(2023, 10, 21),
            PhaseOfEducation = PhaseOfEducation,
            UKPRN = 70000,
            URN = 80000
        };

        schoolContext.Add(School);
        schoolContext.SaveChanges();
        schoolContext.ChangeTracker.Clear();
    }
}
