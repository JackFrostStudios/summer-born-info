namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class CreateSchoolImportRequestTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : SchoolEndpointTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequest_WhenPosted_ThenBackgroundWorkerProcessesTheFile()
    {
        // Arrange
        var client = Factory.CreateClient();
        await using var csvStream = ExampleImportFile.GetExampleImportFileContent();
        using StreamContent content = new(csvStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

        // Act
        var response = await client.PostAsync("/api/schools/import", content, TestContext.Current.CancellationToken);

        // Assert
        _ = response.EnsureSuccessStatusCode();
        var importResponse = await response.Content.ReadFromJsonAsync<ImportSchoolsResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(importResponse);

        var request = await WaitForImportRequestAsync(importResponse.SchoolBulkImportRequestId, TestContext.Current.CancellationToken);
        Assert.NotNull(request);
        Assert.Equal(2, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, request.Status);
        Assert.Empty(request.Failures);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schools = await dbContext.Schools.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, schools.Count);
    }
}
