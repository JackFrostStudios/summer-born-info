namespace SummerBornInfo.TestFramework.Assertions;

public static class EventAssertions
{
    public static async Task AssertEventEqualsAndDeleteAsync<T>(IEventQueue queue, T message, string connectionString, CancellationToken cancellationToken) where T : class
    {
        var npgmq = new NpgmqClient(connectionString);
        var result = await npgmq.ReadAsync<T>(queue.Name, cancellationToken: cancellationToken);
        Assert.NotNull(result);
        Assert.Equal(message, result.Message);
        await npgmq.DeleteAsync(queue.Name, result.MsgId, cancellationToken);
    }

    public static async Task AssertNoEventsExistAsync(IEventQueue queue, string connectionString, CancellationToken cancellationToken)
    {
        var npgmq = new NpgmqClient(connectionString);
        var result = await npgmq.ReadAsync<dynamic>(queue.Name, cancellationToken: cancellationToken);
        Assert.Null(result);
    }
}
