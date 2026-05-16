namespace SummerBornInfo.TestFramework.TestData;

public sealed class LocalAuthorityFactory
{
    public static LocalAuthority GetLocalAuthority()
    {
        (var code, var name) = CodeAndNameFactory.GetCodeAndName();
        return new LocalAuthority()
        {
            Code = code,
            Name = name,
        };
    }
}
