namespace SummerBornInfo.Features.Tests.Schools.Commands.Import;

public sealed class ImportSchoolsCommandHandlerTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequestCommand_WhenExecuted_ThenRequestIsSaved()
    {
        // Arrange
        var injectedDbContext = CreateDbContext();
        var handler = new ImportSchoolsCommandHandler(injectedDbContext, new LargeObjectWriter(injectedDbContext), new EventEmitter(injectedDbContext));
        var command = new ImportSchoolsCommand(ExampleImportFile.GetExampleImportFileContent());

        // Act
        var result = await handler.ExecuteAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, result.SchoolBulkImportRequestId);

        var dbContext = CreateDbContext();
        var savedImportRequest = await dbContext.SchoolBulkImportRequests.FindAsync([result.SchoolBulkImportRequestId], cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedImportRequest);
        await LargeObjectAssertions.AssertLargeObjectExistsAsync(savedImportRequest.ContentId, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
        await LargeObjectAssertions.AssertLargeObjectEqualsOriginalAsync(savedImportRequest.ContentId, command.Content, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
        await AssertSchoolBulkImportEventExistsAsync(savedImportRequest);
    }

    private async Task AssertSchoolBulkImportEventExistsAsync(SchoolBulkImportRequest schoolBulkImportRequest)
    {
        var expectedEvent = new SchoolBulkImportUploaded { SchoolBulkImportRequestId = schoolBulkImportRequest.Id };
        await EventAssertions.AssertEventEqualsAndDeleteAsync(EventQueue.SchoolBulkImport, expectedEvent, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }
}
