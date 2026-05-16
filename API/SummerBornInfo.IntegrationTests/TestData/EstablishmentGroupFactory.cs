namespace SummerBornInfo.TestFramework.TestData;

public sealed class EstablishmentGroupFactory
{
    public static EstablishmentGroup GetEstablishmentGroup()
    {
        (string? code, string? name) = CodeAndNameFactory.GetCodeAndName();
        return new EstablishmentGroup()
        {
            Code = code,
            Name = name
        };
    }
}
