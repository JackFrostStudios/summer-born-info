namespace SummerBornInfo.Infrastructure.Events;

public interface IEventAcknowledger
{
    Task DeleteEventAsync(IEventQueue queue, long messageId, CancellationToken cancellationToken);
}
