namespace SummerBornInfo.Infrastructure.Tests.Persistence.Events;

public class EventEmitterTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNoTransactionHasStarted_WhenEmittingEvent_ThenEventIsCreated()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var eventEmitter = new EventEmitter(dbContext);
        var schoolBulkImportUploadedEvent = new SchoolBulkImportUploaded
        {
            SchoolBulkImportRequestId = Guid.NewGuid()
        };

        // Act
        await eventEmitter.EmitEventAsync(EventQueue.SchoolBulkImport, schoolBulkImportUploadedEvent, TestContext.Current.CancellationToken);

        //Assert
        await EventAssertions.AssertEventEqualsAndDeleteAsync(EventQueue.SchoolBulkImport, schoolBulkImportUploadedEvent, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenTransactionHasStarted_WhenEmittingEvent_ThenEventIsNotPersisted()
    {
        // Arrange
        var dbContext = CreateDbContext();
        await using var efTransaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var eventEmitter = new EventEmitter(dbContext);
        var schoolBulkImportUploadedEvent = new SchoolBulkImportUploaded
        {
            SchoolBulkImportRequestId = Guid.NewGuid()
        };

        // Act
        await eventEmitter.EmitEventAsync(EventQueue.SchoolBulkImport, schoolBulkImportUploadedEvent, TestContext.Current.CancellationToken);
        await efTransaction.RollbackAsync(TestContext.Current.CancellationToken);
        
        //Assert
        await EventAssertions.AssertNoEventsExistAsync(EventQueue.SchoolBulkImport, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenTransactionHasStarted_WhenEmittingEventIsCommitted_ThenEventIsCreated()
    {
        // Arrange
        var dbContext = CreateDbContext();
        await using var efTransaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var eventEmitter = new EventEmitter(dbContext);
        var schoolBulkImportUploadedEvent = new SchoolBulkImportUploaded
        {
            SchoolBulkImportRequestId = Guid.NewGuid()
        };

        // Act
        await eventEmitter.EmitEventAsync(EventQueue.SchoolBulkImport, schoolBulkImportUploadedEvent, TestContext.Current.CancellationToken);
        await efTransaction.CommitAsync(TestContext.Current.CancellationToken);

        //Assert
        await EventAssertions.AssertEventEqualsAndDeleteAsync(EventQueue.SchoolBulkImport, schoolBulkImportUploadedEvent, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }
}
