namespace SummerBornInfo.TestFramework.TestData;

public static class ExampleImportFile
{
    private const string Resource = "SummerBornInfo.TestFramework.TestData.ExampleImportFile.csv";
    public static Stream GetExampleImportFileContent()
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceStream(Resource)
            ?? throw new ExampleImportFileResourceNotFoundException($"Unable to retrieve resource {Resource}");
    }
}
