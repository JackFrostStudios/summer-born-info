namespace SummerBornInfo.TestFramework.TestData;

public sealed class EstablishmentStatusFactory
{
    public static EstablishmentStatus GetEstablishmentStatus()
    {
        (string? code, string? name) = CodeAndNameFactory.GetCodeAndName();
        return new EstablishmentStatus()
        {
            Code = code,
            Name = name
        };
    }
}
