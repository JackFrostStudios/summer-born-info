namespace SummerBornInfo.TestFramework.TestData;

public sealed class LocalAuthorityFactory
{
    public static LocalAuthority GetLocalAuthority()
    {
        var (code, name) = CodeAndNameFactory.GetCodeAndName();
        return new LocalAuthority()
        {
            Code = code,
            Name = name
        };
    }
}
