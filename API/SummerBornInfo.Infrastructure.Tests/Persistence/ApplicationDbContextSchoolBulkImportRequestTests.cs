namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextSchoolBulkImportRequestTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewSchoolBulkImportRequest_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
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
}
