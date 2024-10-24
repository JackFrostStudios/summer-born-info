using SummerBorn.Core.Entity;
using SummerBorn.Core.Entity.Establishment;
using SummerBorn.Infrastructure.Data;

namespace SummerBorn.Infrastructure.Test.DatabaseSetup;
public class SeededData
{
    public readonly PhaseOfEducation PhaseOfEducation;
    public readonly LocalAuthority LocalAuthority;
    public readonly EstablishmentGroup EstablishmentGroup;
    public readonly EstablishmentStatus EstablishmentStatus;
    public readonly EstablishmentType EstablishmentType;
    public readonly School School;

    public SeededData(SchoolContext SchoolContext)
    {
        PhaseOfEducation = new PhaseOfEducation { Code = 10000, Name = "Primary" };
        LocalAuthority = new LocalAuthority { Code = 20000, Name = "Authority" };
        EstablishmentGroup = new EstablishmentGroup { Code = 30000, Name = "Establishment Group" };
        EstablishmentStatus = new EstablishmentStatus { Code = 40000, Name = "Open" };
        EstablishmentType = new EstablishmentType { Code = 50000, Name = "Community School" };
        var address = new Address { Street = "Str", Locality = "loc", AddressThree = "addr3", Town = "twn", County = "Coun", PostCode = "PT12CD" };

        SchoolContext.Add(PhaseOfEducation);
        SchoolContext.Add(LocalAuthority);
        SchoolContext.Add(EstablishmentGroup);
        SchoolContext.Add(EstablishmentStatus);
        SchoolContext.Add(EstablishmentType);
        SchoolContext.SaveChanges();

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

        SchoolContext.Add(School);
        SchoolContext.SaveChanges();
    }
}
