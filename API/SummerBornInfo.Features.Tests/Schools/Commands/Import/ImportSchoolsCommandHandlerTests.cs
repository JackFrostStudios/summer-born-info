namespace SummerBornInfo.Features.Tests.Schools.Commands.Import;

public sealed class ImportSchoolsCommandHandlerTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) 
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequestCommand_WhenExecuted_ThenRequestIsSaved()
    {
        // Arrange
        var handler = new ImportSchoolsCommandHandler(CreateDbContext());
        var command = new ImportSchoolsCommand(ExampleImportFile.GetExampleImportFileContent());

        // Act
        var result = await handler.ExecuteAsync(command, Xunit.TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, result.SchoolBulkImportRequestId);

        var dbContext = CreateDbContext();
        var savedImportRequest = dbContext.SchoolBulkImportRequests.Find(result.SchoolBulkImportRequestId);

        Assert.NotNull(savedImportRequest);
        await AssertLargeObjectExists(savedImportRequest.ContentId);
    }

    private async Task AssertLargeObjectExists(uint objectId)
    {
        await using var conn = new NpgsqlConnection(integrationTestDatabaseInstanceFixture.DatabaseConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
        "SELECT EXISTS(SELECT 1 FROM pg_largeobject_metadata WHERE oid = @oid)",
        conn
        );
        cmd.Parameters.AddWithValue("oid", NpgsqlDbType.Oid, objectId);

        var exists = (bool)(await cmd.ExecuteScalarAsync())!;
        Assert.True(exists);
    }
}
