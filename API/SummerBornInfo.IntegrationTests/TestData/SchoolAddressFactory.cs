namespace SummerBornInfo.TestFramework.TestData;

public sealed class SchoolAddressFactory
{
    private static readonly Faker _faker = new("en_GB");
    public static SchoolAddress GetSchoolAddress()
    {
        return new SchoolAddress()
        {
            AddressThree = _faker.Address.SecondaryAddress(),
            County = _faker.Address.County(),
            Locality = _faker.Address.State(),
            PostCode = _faker.Address.ZipCode(),
            Street = _faker.Address.StreetAddress(),
            Town = _faker.Address.City(),
        };
    }
}
