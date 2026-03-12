namespace SummerBornInfo.TestFramework.TestData;

public static class SchoolFactory
{
    private static int URN = 0;
    private readonly static Faker<School> _faker = new Faker<School>()
        .UseSeed(1)
        .StrictMode(false)
        .RuleFor(s => s.CloseDate, f => f.Date.PastDateOnly().OrNull(f, .8f))
        .RuleFor(s => s.OpenDate, f => f.Date.PastDateOnly().OrNull(f, .1f))
        .RuleFor(s => s.Name, f => f.Company.CompanyName())
        .RuleFor(s => s.UKPRN, f => f.Random.Int(0).OrNull(f, .05f))
        .RuleFor(s => s.URN, f => Interlocked.Increment(ref URN))
        .RuleFor(s => s.Address, f => SchoolAddressFactory.GetSchoolAddress());
    public static School GetSchool()
    {
        return _faker.Generate();
    }
}
