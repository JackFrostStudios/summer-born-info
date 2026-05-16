namespace SummerBornInfo.TestFramework.TestData;

public sealed class EstablishmentTypeFactory
{
    public static EstablishmentType GetEstablishmentType()
    {
        (string? code, string? name) = CodeAndNameFactory.GetCodeAndName();
        return new EstablishmentType()
        {
            Code = code,
            Name = name
        };
    }
}
