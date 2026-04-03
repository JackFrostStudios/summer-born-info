namespace SummerBornInfo.Infrastructure.Events;

public sealed class EventEmitter(ApplicationDbContext dbContext) : IEventEmitter
{
    public async Task EmitEventAsync<T>(IEventQueue queue, T message, CancellationToken cancellationToken) where T : class
    {
        var npgmq = new NpgmqClient(dbContext.GetNpgsqlConnection());
        await npgmq.SendAsync<T>(queue.Name, message, cancellationToken);
    }
}
