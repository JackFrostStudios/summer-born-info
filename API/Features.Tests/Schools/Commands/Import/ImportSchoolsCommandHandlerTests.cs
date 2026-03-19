namespace SummerBornInfo.Features.Tests.Schools.Commands.Import;

public sealed class ImportSchoolsCommandHandlerTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) 
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequestCommand_WhenExecuted_ThenRequestIsSaved()
    {
        // Arrange
        var handler = new ImportSchoolsCommandHandler(CreateDbContext());
        var command = new ImportSchoolsCommand(await ExampleImportFile.GetExampleImportFileContentAsync(Xunit.TestContext.Current.CancellationToken));

        // Act
        var result = await handler.ExecuteAsync(command, Xunit.TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, result.SchoolBulkImportRequestId);

        var dbContext = CreateDbContext();
        var savedImportRequest = dbContext.SchoolBulkImportRequests.Find(result.SchoolBulkImportRequestId);

        Assert.NotNull(savedImportRequest);
        Assert.NotEmpty(savedImportRequest.Content);
    }
}
