namespace SummerBornInfo.Infrastructure.Tests.Persistence.LargeObjects;

public sealed class LargeObjectWriterTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNoTransactionHasStarted_WhenWritingLargeObject_ThenDataIsSavedToLargeObject()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var largeObjectWriter = new LargeObjectWriter(dbContext);
        var largeObjectStream = ExampleImportFile.GetExampleImportFileContent();


        // Act
        var largeObjectId = await largeObjectWriter.StreamContentToNewLargeObjectAsync(largeObjectStream, TestContext.Current.CancellationToken);

        //Assert
        await LargeObjectAssertions.AssertLargeObjectExistsAsync(largeObjectId, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
        await LargeObjectAssertions.AssertLargeObjectEqualsOriginalAsync(largeObjectId, largeObjectStream, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenTransactionHasStarted_WhenWritingLargeObjectAndTransactionIsRolledBack_ThenLargeObjectIsNotPersisted()
    {
        // Arrange
        var dbContext = CreateDbContext();
        await using var efTransaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var largeObjectWriter = new LargeObjectWriter(dbContext);
        var largeObjectStream = ExampleImportFile.GetExampleImportFileContent();


        // Act
        var largeObjectId = await largeObjectWriter.StreamContentToNewLargeObjectAsync(largeObjectStream, TestContext.Current.CancellationToken);
        await efTransaction.RollbackAsync(TestContext.Current.CancellationToken);

        //Assert
        await LargeObjectAssertions.AssertLargeObjectDoesNotExistsAsync(largeObjectId, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenTransactionHasStarted_WhenWritingLargeObjectAndTransactionIsCommitted_ThenDataIsSavedToLargeObject()
    {
        // Arrange
        var dbContext = CreateDbContext();
        await using var efTransaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var largeObjectWriter = new LargeObjectWriter(dbContext);
        var largeObjectStream = ExampleImportFile.GetExampleImportFileContent();


        // Act
        var largeObjectId = await largeObjectWriter.StreamContentToNewLargeObjectAsync(largeObjectStream, TestContext.Current.CancellationToken);
        await efTransaction.CommitAsync(TestContext.Current.CancellationToken);

        //Assert
        await LargeObjectAssertions.AssertLargeObjectExistsAsync(largeObjectId, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
        await LargeObjectAssertions.AssertLargeObjectEqualsOriginalAsync(largeObjectId, largeObjectStream, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }
}