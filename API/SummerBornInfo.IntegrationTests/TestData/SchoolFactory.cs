namespace SummerBornInfo.TestFramework.TestData;

public static class SchoolFactory
{
    private static int URN = 0;
    private static int UKPRN = 999;
    private static int EstablishmentNumber = 9999;
    private readonly static Faker _faker = new("en_GB");

    public static School GetSchool()
    {
        EstablishmentGroup establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        EstablishmentStatus establishmentStatus = EstablishmentStatusFactory.GetEstablishmentStatus();
        EstablishmentType establishmentType = EstablishmentTypeFactory.GetEstablishmentType();
        LocalAuthority localAuthority = LocalAuthorityFactory.GetLocalAuthority();
        PhaseOfEducation phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();

        var closeDate = _faker.Date.PastDateOnly(1);
        var openDate = _faker.Date.PastDateOnly(100, closeDate);

        return new School()
        {
            CloseDate = closeDate,
            OpenDate = openDate,
            Name = _faker.Company.CompanyName(),
            UKPRN = Interlocked.Increment(ref UKPRN),
            URN = Interlocked.Increment(ref URN),
            EstablishmentNumber = Interlocked.Increment(ref EstablishmentNumber),
            Address = SchoolAddressFactory.GetSchoolAddress(),
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus,
            EstablishmentType = establishmentType,
            LocalAuthority = localAuthority,
            PhaseOfEducation = phaseOfEducation
        };
    }
}
