namespace SummerBornInfo.Infrastructure.Tests.Persistence.Events;

public class EventReaderTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNoEventExists_WhenReadingEvent_ThenNullIsReturned()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var eventEmitter = new EventReader(dbContext);

        // Act
        var result = await eventEmitter.ReadEventAsync<TestEvent>(TestEventQueue.TestQueue, 10, TestContext.Current.CancellationToken);

        //Assert
        Assert.Null(result);
    }
}
