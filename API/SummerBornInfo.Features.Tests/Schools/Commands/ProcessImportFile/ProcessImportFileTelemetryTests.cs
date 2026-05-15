using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;
using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile;

public sealed class ProcessImportFileTelemetryTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequestWithMixedRows_WhenExecuted_ThenEachProcessedRowEmitsActivity()
    {
        // Arrange
        using var invalidCsv = CreateCsvStream(
            "\"URN\",\"EstablishmentNumber\",\"EstablishmentName\",\"LA (code)\",\"LA (name)\",\"TypeOfEstablishment (code)\",\"TypeOfEstablishment (name)\",\"EstablishmentTypeGroup (code)\",\"EstablishmentTypeGroup (name)\",\"EstablishmentStatus (code)\",\"EstablishmentStatus (name)\",\"PhaseOfEducation (code)\",\"PhaseOfEducation (name)\",\"OpenDate\",\"CloseDate\",\"UKPRN\",\"Street\",\"Locality\",\"Address3\",\"Town\",\"County (name)\",\"Postcode\"",
            "\"100000\",\"3614\",\"The Aldgate School\",\"201\",\"City of London\",\"02\",\"Voluntary aided school\",\"4\",\"Local authority maintained schools\",\"1\",\"Open\",\"2\",\"Primary\",\"\",\"\",\"10079319\",\"St James's Passage\",\"Duke's Place\",\"\",\"London\",\"\",\"EC3A 5DE\"",
            "\"INVALID\",\"1045\",\"Broken School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"",
            "\"100004\",\"1045\",\"Sherborne Nursery School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"");
        var requestId = await CreateImportRequestAsync(invalidCsv);
        var handler = CreateHandler(CreateDbContext());
        var capturedActivities = new List<Activity>();

        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == SchoolBulkImportTelemetry.ActivitySourceName,
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

        // Act
        await handler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken);

        // Assert
        var verifyDbContext = CreateDbContext();
        var request = await verifyDbContext.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);

        Assert.Equal(3, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.CompletedWithFailures, request.Status);
        Assert.Single(request.Failures);

        var schools = await verifyDbContext.Schools
            .OrderBy(x => x.URN)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal([100000, 100004], schools.Select(x => x.URN).ToArray());

        List<Activity> processActivities;
        lock (capturedActivities)
        {
            processActivities = capturedActivities
                .Where(activity => activity.OperationName == SchoolBulkImportTelemetry.ActivityName
                    && Equals(activity.GetTagItem("schoolBulkImport.request_id"), requestId))
                .ToList();
        }

        Assert.Equal(3, processActivities.Count);
        Assert.All(processActivities, activity => Assert.Equal(requestId, activity.GetTagItem("schoolBulkImport.request_id")));
        Assert.Contains(processActivities, activity => Equals(activity.GetTagItem("schoolBulkImport.outcome"), "processed"));
        Assert.Contains(processActivities, activity => Equals(activity.GetTagItem("schoolBulkImport.outcome"), "failed"));
    }

    private ProcessImportFileCommandHandler CreateHandler(ApplicationDbContext dbContext) =>
        new(dbContext, new LargeObjectReader(dbContext), new SchoolsImporter<ApplicationDbContext>(dbContext));

    private async Task<Guid> CreateImportRequestAsync(Stream content)
    {
        var dbContext = CreateDbContext();
        var writer = new LargeObjectWriter(dbContext);
        var largeObjectId = await writer.StreamContentToNewLargeObjectAsync(content, TestContext.Current.CancellationToken);

        var request = new SchoolBulkImportRequest
        {
            ContentId = largeObjectId,
        };

        dbContext.SchoolBulkImportRequests.Add(request);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return request.Id;
    }

    private static MemoryStream CreateCsvStream(params string[] lines) =>
        new(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)));
}
