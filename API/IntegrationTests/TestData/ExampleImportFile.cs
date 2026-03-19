namespace SummerBornInfo.TestFramework.TestData;

public static class ExampleImportFile
{
    const string Resource = "SummerBornInfo.TestFramework.TestData.ExampleImportFile.csv";
    const string LargeResource = "SummerBornInfo.TestFramework.TestData.ExampleLargeImportFile.csv";
    public static async Task<byte[]> GetExampleImportFileContentAsync(CancellationToken cancellation)
    {
        return await GetFileContentAsync(Resource, cancellation);
    }

    public static async Task<byte[]> GetExampleLargeImportFileContentAsync(CancellationToken cancellation)
    {
        return await GetFileContentAsync(LargeResource, cancellation);
    }

    private static async Task<byte[]> GetFileContentAsync(string resource, CancellationToken cancellation)
    {
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
        {
            if (stream != null)
            {
                byte[] content = new byte[stream.Length];
                await stream.ReadExactlyAsync(content, 0, content.Length, cancellation);
                return content;
            }
        }

        return [];
    }
}
