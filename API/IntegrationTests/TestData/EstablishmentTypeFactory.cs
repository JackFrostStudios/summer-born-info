namespace SummerBornInfo.TestFramework.TestData;

public sealed class EstablishmentTypeFactory
{
    public static EstablishmentType GetEstablishmentType()
    {
        var (code, name) = CodeAndNameFactory.GetCodeAndName();
        return new EstablishmentType()
        {
            Code = code,
            Name = name
        };
    }
}
