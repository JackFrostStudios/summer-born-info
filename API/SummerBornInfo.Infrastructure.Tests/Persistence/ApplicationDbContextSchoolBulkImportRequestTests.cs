using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextSchoolBulkImportRequestTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewSchoolBulkImportRequest_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var schoolBulkImportRequest = new SchoolBulkImportRequest()
        {
            ContentId = 1,
        };

        // Act
        dbContext.SchoolBulkImportRequests.Add(schoolBulkImportRequest);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchoolBulkImportRequest = dbContext.SchoolBulkImportRequests.Find(schoolBulkImportRequest.Id);

        Assert.NotNull(savedSchoolBulkImportRequest);
        Assert.Equivalent(schoolBulkImportRequest, savedSchoolBulkImportRequest);
    }

    [Fact]
    public async Task GivenSchoolBulkImportRequestWithProgressAndFailure_WhenInsertingToDatabase_ThenAllStateIsRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 7,
        };
        schoolBulkImportRequest.ProcessingStarted();
        for (int lineNumber = 1; lineNumber <= 11; lineNumber++)
        {
            schoolBulkImportRequest.UpdateProgress(lineNumber, null);
        }

        schoolBulkImportRequest.UpdateProgress(8, "URN is required");
        schoolBulkImportRequest.ProcessingComplete();

        // Act
        dbContext.SchoolBulkImportRequests.Add(schoolBulkImportRequest);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        dbContext.ChangeTracker.Clear();

        var savedSchoolBulkImportRequest = await dbContext.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleAsync(x => x.Id == schoolBulkImportRequest.Id, TestContext.Current.CancellationToken);

        Assert.Equal(7u, savedSchoolBulkImportRequest.ContentId);
        Assert.Equal(12, savedSchoolBulkImportRequest.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.CompletedWithFailures, savedSchoolBulkImportRequest.Status);
        Assert.Single(savedSchoolBulkImportRequest.Failures);
        Assert.Equal(8, savedSchoolBulkImportRequest.Failures[0].LineNumber);
        Assert.Equal("URN is required", savedSchoolBulkImportRequest.Failures[0].ErrorMessage);
    }
}
