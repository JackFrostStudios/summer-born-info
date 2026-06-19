namespace SummerBornInfo.TestFramework.TestData;

public static class SchoolFactory
{
    private static int _urn;
    private static int _ukprn = 999;
    private static int _establishmentNumber = 9999;
    private static readonly Faker _faker = new("en_GB");

    public static School GetSchool()
    {
        var establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        var establishmentStatus = EstablishmentStatusFactory.GetEstablishmentStatus();
        var establishmentType = EstablishmentTypeFactory.GetEstablishmentType();
        var localAuthority = LocalAuthorityFactory.GetLocalAuthority();
        var phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();

        var closeDate = _faker.Date.PastDateOnly(1);
        var openDate = _faker.Date.PastDateOnly(100, closeDate);

        return new School()
        {
            CloseDate = closeDate,
            OpenDate = openDate,
            Name = _faker.Company.CompanyName(),
            UKPRN = Interlocked.Increment(ref _ukprn),
            URN = Interlocked.Increment(ref _urn),
            EstablishmentNumber = Interlocked.Increment(ref _establishmentNumber),
            Address = SchoolAddressFactory.GetSchoolAddress(),
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus,
            EstablishmentType = establishmentType,
            LocalAuthority = localAuthority,
            PhaseOfEducation = phaseOfEducation,
        };
    }
}
