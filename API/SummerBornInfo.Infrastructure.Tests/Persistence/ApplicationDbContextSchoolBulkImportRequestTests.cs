namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextSchoolBulkImportRequestTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewSchoolBulkImportRequest_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        SchoolBulkImportRequest schoolBulkImportRequest = new()
        {
            ContentId = 1,
        };

        // Act
        _ = dbContext.SchoolBulkImportRequests.Add(schoolBulkImportRequest);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchoolBulkImportRequest = await dbContext.SchoolBulkImportRequests.FindAsync([schoolBulkImportRequest.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchoolBulkImportRequest);
        Assert.Equivalent(schoolBulkImportRequest, savedSchoolBulkImportRequest);
    }

    [Fact]
    public async Task GivenSchoolBulkImportRequestWithProgressAndFailure_WhenInsertingToDatabase_ThenAllStateIsRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        SchoolBulkImportRequest schoolBulkImportRequest = new()
        {
            ContentId = 7,
        };
        _ = schoolBulkImportRequest.ProcessingStarted();
        for (var lineNumber = 1; lineNumber <= 11; lineNumber++)
        {
            schoolBulkImportRequest.UpdateProgress(lineNumber, errorMessage: null);
        }

        schoolBulkImportRequest.UpdateProgress(8, "URN is required");
        schoolBulkImportRequest.ProcessingComplete();

        // Act
        _ = dbContext.SchoolBulkImportRequests.Add(schoolBulkImportRequest);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        dbContext.ChangeTracker.Clear();

        var savedSchoolBulkImportRequest = await dbContext.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleAsync(x => x.Id == schoolBulkImportRequest.Id, TestContext.Current.CancellationToken);

        Assert.Equal(7u, savedSchoolBulkImportRequest.ContentId);
        Assert.Equal(12, savedSchoolBulkImportRequest.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.CompletedWithFailures, savedSchoolBulkImportRequest.Status);
        _ = Assert.Single(savedSchoolBulkImportRequest.Failures);
        Assert.Equal(8, savedSchoolBulkImportRequest.Failures[0].LineNumber);
        Assert.Equal("URN is required", savedSchoolBulkImportRequest.Failures[0].ErrorMessage);
    }
}
