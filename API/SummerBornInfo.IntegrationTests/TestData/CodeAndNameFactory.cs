namespace SummerBornInfo.TestFramework.TestData;

public static class CodeAndNameFactory
{
    private static readonly Faker _faker = new("en_GB");
    public static (string code, string name) GetCodeAndName()
    {
        var words = _faker.Lorem.Words();
        var name = string.Join(" ", words);
        var code = string.Join("", words).ToUpperInvariant();
        return (code, name);
    }
}
