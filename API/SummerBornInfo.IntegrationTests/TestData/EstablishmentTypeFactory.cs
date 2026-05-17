namespace SummerBornInfo.TestFramework.TestData;

public static class EstablishmentTypeFactory
{
    public static EstablishmentType GetEstablishmentType()
    {
        (var code, var name) = CodeAndNameFactory.GetCodeAndName();
        return new EstablishmentType()
        {
            Code = code,
            Name = name,
        };
    }
}
