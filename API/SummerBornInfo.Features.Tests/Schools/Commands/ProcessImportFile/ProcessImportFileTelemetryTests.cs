namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile;

public sealed class ProcessImportFileTelemetryTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequestWithMixedRows_WhenExecuted_ThenEachProcessedRowEmitsActivity()
    {
        await
                // Arrange
                using var invalidCsv = CreateCsvStream(
            "\"URN\",\"EstablishmentNumber\",\"EstablishmentName\",\"LA (code)\",\"LA (name)\",\"TypeOfEstablishment (code)\",\"TypeOfEstablishment (name)\",\"EstablishmentTypeGroup (code)\",\"EstablishmentTypeGroup (name)\",\"EstablishmentStatus (code)\",\"EstablishmentStatus (name)\",\"PhaseOfEducation (code)\",\"PhaseOfEducation (name)\",\"OpenDate\",\"CloseDate\",\"UKPRN\",\"Street\",\"Locality\",\"Address3\",\"Town\",\"County (name)\",\"Postcode\"",
            "\"100000\",\"3614\",\"The Aldgate School\",\"201\",\"City of London\",\"02\",\"Voluntary aided school\",\"4\",\"Local authority maintained schools\",\"1\",\"Open\",\"2\",\"Primary\",\"\",\"\",\"10079319\",\"St James's Passage\",\"Duke's Place\",\"\",\"London\",\"\",\"EC3A 5DE\"",
            "\"INVALID\",\"1045\",\"Broken School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"",
            "\"100004\",\"1045\",\"Sherborne Nursery School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"");
        var requestId = await CreateImportRequestAsync(invalidCsv);
        var handler = CreateHandler(CreateDbContext());
        List<Activity> capturedActivities = [];

        using ActivityListener activityListener = new()
        {
            ShouldListenTo = source => string.Equals(source.Name, SchoolBulkImportTelemetry.ActivitySourceName, StringComparison.Ordinal),
            SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                lock (capturedActivities)
                {
                    capturedActivities.Add(activity);
                }
            },
        };

        ActivitySource.AddActivityListener(activityListener);

        // Act
        await handler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken);

        // Assert
        List<Activity> processActivities;
        lock (capturedActivities)
        {
            processActivities = [
                .. capturedActivities
                    .Where(activity =>
                        string.Equals(activity.OperationName, SchoolBulkImportTelemetry.ActivityName, StringComparison.Ordinal)
                        && Equals(activity.GetTagItem("schoolBulkImport.request_id"), requestId)
                    ),
            ];
        }

        Assert.Equal(3, processActivities.Count);
        Assert.All(processActivities, activity => Assert.Equal(requestId, activity.GetTagItem("schoolBulkImport.request_id")));
        Assert.Contains(processActivities, activity => Equals(activity.GetTagItem("schoolBulkImport.outcome"), "processed"));
        Assert.Contains(processActivities, activity => Equals(activity.GetTagItem("schoolBulkImport.outcome"), "failed"));
    }

    private static ProcessImportFileCommandHandler CreateHandler(ApplicationDbContext dbContext)
    {
        return new(
            dbContext,
            new LargeObjectReader(dbContext),
            new SchoolsImporter<ApplicationDbContext>(
                dbContext,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<SchoolsImporter<ApplicationDbContext>>.Instance));
    }

    private async Task<Guid> CreateImportRequestAsync(Stream content)
    {
        var dbContext = CreateDbContext();
        LargeObjectWriter writer = new(dbContext);
        var largeObjectId = await writer.StreamContentToNewLargeObjectAsync(content, TestContext.Current.CancellationToken);

        SchoolBulkImportRequest request = new()
        {
            ContentId = largeObjectId,
        };

        _ = dbContext.SchoolBulkImportRequests.Add(request);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return request.Id;
    }

    private static MemoryStream CreateCsvStream(params string[] lines)
    {
        return new(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)));
    }
}
