using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence.Events;

public sealed class EventAcknowledgerTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenQueuedEventExists_WhenDeleted_ThenItIsNotReturnedAgain()
    {
        // Arrange
        var eventReader = new EventReader(CreateDbContext());
        var eventAcknowledger = new EventAcknowledger(CreateDbContext());
        ApplicationDbContext eventEmitterDbContext = CreateDbContext();
        var eventEmitter = new EventEmitter(eventEmitterDbContext);

        await using var dbContextTransaction = await eventEmitterDbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var testEvent = new TestEvent();
        await eventEmitter.EmitEventAsync(TestEventQueue.TestQueue, testEvent, TestContext.Current.CancellationToken);
        await dbContextTransaction.CommitAsync(TestContext.Current.CancellationToken);

        var queuedEvent = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 10, TestContext.Current.CancellationToken);
        Assert.NotNull(queuedEvent);

        // Act
        await eventAcknowledger.DeleteEventAsync(TestEventQueue.TestQueue, queuedEvent.MessageId, TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        var rereadEvent = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 1, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(rereadEvent);
    }
}
