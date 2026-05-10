namespace SummerBornInfo.Infrastructure.Tests.Persistence.Events;

public class EventReaderTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNoEventExists_WhenReadingEvent_ThenNullIsReturned()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var eventReader = new EventReader(dbContext);

        // Act
        var result = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 10, TestContext.Current.CancellationToken);

        //Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GivenAnEventExists_WhenReadingEvent_ThenTheEventIsReturned()
    {
        // Arrange
        var eventReaderDbContext = CreateDbContext();
        var eventReader = new EventReader(eventReaderDbContext);
        var eventEmitterDbContext = CreateDbContext();
        var eventEmitter = new EventEmitter(eventEmitterDbContext);
        
        await using var dbContextTransaction = await eventEmitterDbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var testEvent = new TestEvent();
        await eventEmitter.EmitEventAsync(TestEventQueue.TestQueue, testEvent, TestContext.Current.CancellationToken);
        await dbContextTransaction.CommitAsync(TestContext.Current.CancellationToken);
        
        // Act
        var result = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 10, TestContext.Current.CancellationToken);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(testEvent, result.Message);
        Assert.True(result.MessageId > 0);
    }

    [Fact]
    public async Task GivenEventHasBeenRead_WhenTimeoutExpires_ThenTheEventCanBeRetrievedAgain()
    {
        // Arrange
        var eventReaderDbContext = CreateDbContext();
        var eventReader = new EventReader(eventReaderDbContext);
        var eventEmitterDbContext = CreateDbContext();
        var eventEmitter = new EventEmitter(eventEmitterDbContext);

        await using var dbContextTransaction = await eventEmitterDbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var testEvent = new TestEvent();
        await eventEmitter.EmitEventAsync(TestEventQueue.TestQueue, testEvent, TestContext.Current.CancellationToken);
        await dbContextTransaction.CommitAsync(TestContext.Current.CancellationToken);
        var initialRetrieval = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 1, TestContext.Current.CancellationToken);
        Assert.NotNull(initialRetrieval);
        Assert.Equal(testEvent, initialRetrieval.Message);

        // Act
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        var result = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 10, TestContext.Current.CancellationToken);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(testEvent, result.Message);
    }

    [Fact]
    public async Task GivenEventHasBeenReadAndDeleted_WhenReadingAgain_ThenEventIsNotReturned()
    {
        // Arrange
        var eventReaderDbContext = CreateDbContext();
        var eventReader = new EventReader(eventReaderDbContext);
        var eventEmitterDbContext = CreateDbContext();
        var eventEmitter = new EventEmitter(eventEmitterDbContext);

        await using var dbContextTransaction = await eventEmitterDbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var testEvent = new TestEvent();
        await eventEmitter.EmitEventAsync(TestEventQueue.TestQueue, testEvent, TestContext.Current.CancellationToken);
        await dbContextTransaction.CommitAsync(TestContext.Current.CancellationToken);

        var queuedEvent = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 10, TestContext.Current.CancellationToken);
        Assert.NotNull(queuedEvent);

        // Act
        await eventReader.DeleteEventAsync(TestEventQueue.TestQueue, queuedEvent.MessageId, TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        var result = await eventReader.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 1, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }
}
