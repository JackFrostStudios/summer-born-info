namespace SummerBornInfo.Infrastructure.Tests.Persistence.Events;

public class EventEmitterTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNoTransactionHasStarted_WhenEmittingEvent_ThenEventIsCreated()
    {
        // Arrange
        var dbContext = CreateDbContext();
        EventEmitter eventEmitter = new(dbContext);
        TestEvent testEvent = new();

        // Act
        await eventEmitter.EmitEventAsync(TestEventQueue.TestQueue, testEvent, TestContext.Current.CancellationToken);

        //Assert
        await EventAssertions.AssertEventEqualsAndDeleteAsync(TestEventQueue.TestQueue, testEvent, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenTransactionHasStarted_WhenEmittingEvent_ThenEventIsNotPersisted()
    {
        // Arrange
        var dbContext = CreateDbContext();
        await using var efTransaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        EventEmitter eventEmitter = new(dbContext);
        TestEvent testEvent = new();

        // Act
        await eventEmitter.EmitEventAsync(TestEventQueue.TestQueue, testEvent, TestContext.Current.CancellationToken);
        await efTransaction.RollbackAsync(TestContext.Current.CancellationToken);

        //Assert
        await EventAssertions.AssertNoEventsExistAsync(TestEventQueue.TestQueue, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenTransactionHasStarted_WhenEmittingEventIsCommitted_ThenEventIsCreated()
    {
        // Arrange
        var dbContext = CreateDbContext();
        await using var efTransaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        EventEmitter eventEmitter = new(dbContext);
        TestEvent testEvent = new();

        // Act
        await eventEmitter.EmitEventAsync(TestEventQueue.TestQueue, testEvent, TestContext.Current.CancellationToken);
        await efTransaction.CommitAsync(TestContext.Current.CancellationToken);

        //Assert
        await EventAssertions.AssertEventEqualsAndDeleteAsync(TestEventQueue.TestQueue, testEvent, integrationTestDatabaseInstanceFixture.DatabaseConnectionString, TestContext.Current.CancellationToken);
    }
}
