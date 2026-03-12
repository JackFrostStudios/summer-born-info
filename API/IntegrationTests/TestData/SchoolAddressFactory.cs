namespace SummerBornInfo.TestFramework.TestData;

internal class SchoolAddressFactory
{
    private readonly static Faker<SchoolAddress> _faker = new Faker<SchoolAddress>("en_GB")
        .UseSeed(2)
        .StrictMode(false)
        .RuleFor(a => a.AddressThree, f => f.Address.SecondaryAddress().OrNull(f))
        .RuleFor(a => a.County, f => f.Address.County().OrNull(f))
        .RuleFor(a => a.Locality, f => f.Address.State().OrNull(f))
        .RuleFor(a => a.PostCode, f => f.Address.ZipCode())
        .RuleFor(a => a.Street, f => f.Address.StreetAddress().OrNull(f))
        .RuleFor(a => a.Town, f => f.Address.City().OrNull(f));

    public static SchoolAddress GetSchoolAddress()
    {
        return _faker.Generate();
    }
}
