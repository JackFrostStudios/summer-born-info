namespace SummerBornInfo.Infrastructure.Events;

public sealed class EventReader(ApplicationDbContext dbContext) : IEventReader
{
    public async Task<QueuedEvent<T>?> ReadEventAsync<T>(IEventQueue queue, int messageReadTimeoutSeconds, CancellationToken cancellationToken) where T : class
    {
        var npgmq = new NpgmqClient(dbContext.GetNpgsqlConnection());
        var message = await npgmq.ReadAsync<T>(queue.Name, messageReadTimeoutSeconds, cancellationToken);
        return message is null
            ? null
            : new QueuedEvent<T>(message.MsgId, message.Message ?? throw new InvalidOperationException("Queue message payload was null."));
    }
}
