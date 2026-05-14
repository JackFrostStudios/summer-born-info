using System.Net.Http.Json;
using SummerBornInfo.Domain.Entities;
using SummerBornInfo.Features.Schools.Commands.Import;
using SummerBornInfo.TestFramework.TestData;

namespace SummerBornInfo.Web.Tests.BackgroundServices;

public sealed class ProcessSchoolBulkImportTelemetryTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenQueuedSchoolBulkImport_WhenBackgroundWorkerProcessesMessage_ThenProcessSchoolBulkImportActivityIsEmitted()
    {
        // Arrange
        var client = factory.CreateClient();
        var capturedActivities = new List<Activity>();

        using var activityListener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                lock (capturedActivities)
                {
                    capturedActivities.Add(activity);
                }
            }
        };

        ActivitySource.AddActivityListener(activityListener);

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

        Activity? processActivity;
        lock (capturedActivities)
        {
            processActivity = capturedActivities.SingleOrDefault(activity => activity.OperationName == "ProcessSchoolBulkImport");
        }

        Assert.NotNull(processActivity);
        Assert.Equal(importResponse.SchoolBulkImportRequestId.ToString(), processActivity.GetTagItem("schoolBulkImport.request_id"));
        Assert.Equal("SchoolBulkImport", processActivity.GetTagItem("messaging.destination"));
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
