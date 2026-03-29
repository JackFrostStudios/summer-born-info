using Npgmq;
using SummerBornInfo.Domain.Entities;
using SummerBornInfo.Domain.Events;
using SummerBornInfo.Infrastructure.Events;
using SummerBornInfo.Infrastructure.Persistence.LargeObjects;

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
        var result = await handler.ExecuteAsync(command, Xunit.TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, result.SchoolBulkImportRequestId);

        var dbContext = CreateDbContext();
        var savedImportRequest = await dbContext.SchoolBulkImportRequests.FindAsync([ result.SchoolBulkImportRequestId ], cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Assert.NotNull(savedImportRequest);
        await AssertLargeObjectExists(savedImportRequest.ContentId);
        await AssertSchoolBulkImportEventExists(savedImportRequest);
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

    private async Task AssertSchoolBulkImportEventExists(SchoolBulkImportRequest schoolBulkImportRequest)
    {
        var npgmq = new NpgmqClient(integrationTestDatabaseInstanceFixture.DatabaseConnectionString);
        var result = await npgmq.ReadAsync<SchoolBulkImportUploaded>(EventQueues.SchoolBulkImport, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(schoolBulkImportRequest.Id, result?.Message?.SchoolBulkImportRequestId);
    }
}
