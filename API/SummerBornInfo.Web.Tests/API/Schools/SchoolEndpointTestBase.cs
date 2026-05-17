namespace SummerBornInfo.Web.Tests.API.Schools;

public abstract class SchoolEndpointTestBase(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    protected async Task<SchoolBulkImportRequest?> WaitForImportRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < TimeSpan.FromSeconds(15))
        {
            await using var scope = Factory.Services.CreateAsyncScope();
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

    protected static SchoolBulkImportRequest CreateImportRequestInStatus(Guid requestId, SchoolBulkImportStatus status)
    {
        SchoolBulkImportRequest request = new()
        {
            Id = requestId,
            ContentId = 999,
        };

        if (status is SchoolBulkImportStatus.Pending)
        {
            return request;
        }

        _ = request.ProcessingStarted();

        if (status is SchoolBulkImportStatus.Processing)
        {
            return request;
        }

        if (status is SchoolBulkImportStatus.Completed)
        {
            request.UpdateProgress(1, errorMessage: null);
            request.ProcessingComplete();
            return request;
        }

        if (status is SchoolBulkImportStatus.CompletedWithFailures)
        {
            request.UpdateProgress(1, "failure");
            request.ProcessingComplete();
            return request;
        }

        request.ProcessingFailed();
        return request;
    }
}
