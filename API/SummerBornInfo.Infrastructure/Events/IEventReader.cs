namespace SummerBornInfo.Infrastructure.Events;

public interface IEventReader
{
    Task<QueuedEvent<T>?> ReadEventAsync<T>(IEventQueue queue, int messageReadTimeoutSeconds, CancellationToken cancellationToken) where T : class;
    Task DeleteEventAsync(IEventQueue queue, long messageId, CancellationToken cancellationToken);
}
