namespace SummerBornInfo.Features.Tests.Schools.Commands.Import;

public sealed class ImportSchoolsCommandHandlerTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequestCommand_WhenExecuted_ThenRequestIsSaved()
    {
        // Arrange
        var injectedDbContext = CreateDbContext();
        ImportSchoolsCommandHandler handler = new(injectedDbContext, new LargeObjectWriter(injectedDbContext), new EventEmitter(injectedDbContext));
        ImportSchoolsCommand command = new(ExampleImportFile.GetExampleImportFileContent());

        // Act
        var result = await handler.ExecuteAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);

        var dbContext = CreateDbContext();
        var savedImportRequest = await dbContext.SchoolBulkImportRequests.FindAsync([result.Id], cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedImportRequest);
        await LargeObjectAssertions.AssertLargeObjectExistsAsync(savedImportRequest.ContentId, IntegrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
        await LargeObjectAssertions.AssertLargeObjectEqualsOriginalAsync(savedImportRequest.ContentId, command.Content, IntegrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
        await AssertSchoolBulkImportEventExistsAsync(savedImportRequest);
    }

    private async Task AssertSchoolBulkImportEventExistsAsync(SchoolBulkImportRequest schoolBulkImportRequest)
    {
        SchoolBulkImportUploaded expectedEvent = new() { SchoolBulkImportRequestId = schoolBulkImportRequest.Id };
        await EventAssertions.AssertEventEqualsAndDeleteAsync(EventQueue.SchoolBulkImport, expectedEvent, IntegrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }
}
