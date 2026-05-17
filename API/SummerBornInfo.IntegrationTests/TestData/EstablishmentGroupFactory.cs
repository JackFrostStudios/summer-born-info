namespace SummerBornInfo.TestFramework.TestData;

public static class EstablishmentGroupFactory
{
    public static EstablishmentGroup GetEstablishmentGroup()
    {
        (var code, var name) = CodeAndNameFactory.GetCodeAndName();
        return new EstablishmentGroup()
        {
            Code = code,
            Name = name,
        };
    }
}
