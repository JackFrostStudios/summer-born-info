namespace SummerBornInfo.Infrastructure.Events;

public sealed class EventAcknowledger(ApplicationDbContext dbContext) : IEventAcknowledger
{
    public async Task DeleteEventAsync(IEventQueue queue, long messageId, CancellationToken cancellationToken)
    {
        var npgmq = new NpgmqClient(dbContext.GetNpgsqlConnection());
        await npgmq.DeleteAsync(queue.Name, messageId, cancellationToken);
    }
}
