namespace SummerBornInfo.Infrastructure.Events;

public interface IEventEmitter
{
    Task EmitEventAsync<T>(IEventQueue queue, T message, CancellationToken cancellationToken) where T : class;
}
