namespace SummerBornInfo.TestFramework.TestData;

public static class PhaseOfEducationFactory
{
    public static PhaseOfEducation GetPhaseOfEducation()
    {
        (var code, var name) = CodeAndNameFactory.GetCodeAndName();
        return new PhaseOfEducation()
        {
            Code = code,
            Name = name,
        };
    }
}
