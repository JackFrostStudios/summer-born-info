namespace SummerBornInfo.TestFramework.TestData;

public sealed class EstablishmentStatusFactory
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
