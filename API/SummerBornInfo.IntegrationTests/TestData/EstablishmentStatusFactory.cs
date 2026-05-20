namespace SummerBornInfo.TestFramework.TestData;

public static class EstablishmentStatusFactory
{
    public static EstablishmentStatus GetEstablishmentStatus()
    {
        (var code, var name) = CodeAndNameFactory.GetCodeAndName();
        return new EstablishmentStatus()
        {
            Code = code,
            Name = name,
        };
    }
}
