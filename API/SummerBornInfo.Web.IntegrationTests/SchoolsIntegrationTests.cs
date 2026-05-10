using System.Net.Http.Json;
using SummerBornInfo.Domain.Entities;
using SummerBornInfo.Features.Schools.Commands.Import;
using SummerBornInfo.TestFramework.TestData;

namespace SummerBornInfo.Web.Tests;

public sealed class SchoolsIntegrationTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GetAllSchoolsQuery()
    {
        var client = factory.CreateClient();
        var result = await client.GetAsync("/api/schools", TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GivenImportRequest_WhenPosted_ThenBackgroundWorkerProcessesTheFile()
    {
        // Arrange
        var client = factory.CreateClient();
        using var csvStream = ExampleImportFile.GetExampleImportFileContent();
        using var content = new StreamContent(csvStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

        // Act
        var response = await client.PostAsync("/api/schools/import", content, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var importResponse = await response.Content.ReadFromJsonAsync<ImportSchoolsResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(importResponse);

        var request = await WaitForImportRequestAsync(importResponse.SchoolBulkImportRequestId, TestContext.Current.CancellationToken);
        Assert.NotNull(request);
        Assert.Equal(2, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, request.Status);
        Assert.Empty(request.Failures);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schools = await dbContext.Schools.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, schools.Count);
    }

    private async Task<SchoolBulkImportRequest?> WaitForImportRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < TimeSpan.FromSeconds(15))
        {
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = await dbContext.SchoolBulkImportRequests
                .Include(x => x.Failures)
                .SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);

            if (request?.Status is SchoolBulkImportStatus.Completed or SchoolBulkImportStatus.CompletedWithFailures or SchoolBulkImportStatus.Failed)
            {
                return request;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }

        return null;
    }
}
