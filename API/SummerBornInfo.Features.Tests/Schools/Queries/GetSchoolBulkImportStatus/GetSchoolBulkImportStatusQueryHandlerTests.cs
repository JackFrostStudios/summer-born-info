namespace SummerBornInfo.Features.Tests.Schools.Queries.GetSchoolBulkImportStatus;

public sealed class GetSchoolBulkImportStatusQueryHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenRequestDoesNotExist_WhenExecuteAsync_ThenReturnsNull()
    {
        // Arrange
        var handler = new GetSchoolBulkImportStatusQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetSchoolBulkImportStatusQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GivenRequestExistsWithFailuresOutOfOrder_WhenExecuteAsync_ThenReturnsMappedResponseWithFailuresSortedByLineNumber()
    {
        // Arrange
        var request = new SchoolBulkImportRequest
        {
            Id = Guid.CreateVersion7(),
            ContentId = 42,
        };

        _ = request.ProcessingStarted();
        request.UpdateProgress(10, "Line 10 failed");
        request.UpdateProgress(2, "Line 2 failed");
        request.ProcessingComplete();

        await SaveImportRequestAsync(request);

        var handler = new GetSchoolBulkImportStatusQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetSchoolBulkImportStatusQuery(request.Id),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Id, result.SchoolBulkImportRequestId);
        Assert.Equal(SchoolBulkImportStatus.CompletedWithFailures.ToString(), result.Status);
        Assert.Equal(2, result.LinesProcessed);
        Assert.Equal([2, 10], [.. result.Failures.Select(x => x.LineNumber)]);
        Assert.Equal(["Line 2 failed", "Line 10 failed"], [.. result.Failures.Select(x => x.Message)]);
    }

    [Fact]
    public async Task GivenRequestExistsWithoutFailures_WhenExecuteAsync_ThenReturnsEmptyFailuresCollection()
    {
        // Arrange
        var request = new SchoolBulkImportRequest
        {
            Id = Guid.CreateVersion7(),
            ContentId = 7,
        };

        _ = request.ProcessingStarted();
        request.UpdateProgress(1, errorMessage: null);
        request.ProcessingComplete();

        await SaveImportRequestAsync(request);

        var handler = new GetSchoolBulkImportStatusQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetSchoolBulkImportStatusQuery(request.Id),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SchoolBulkImportStatus.Completed.ToString(), result.Status);
        Assert.Equal(1, result.LinesProcessed);
        Assert.Empty(result.Failures);
    }

    private async Task SaveImportRequestAsync(SchoolBulkImportRequest request)
    {
        var dbContext = CreateDbContext();
        _ = dbContext.SchoolBulkImportRequests.Add(request);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
