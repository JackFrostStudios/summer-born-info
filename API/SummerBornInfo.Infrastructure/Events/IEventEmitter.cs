namespace SummerBornInfo.Infrastructure.Events;

public interface IEventEmitter
{
    Task EmitEventAsync<T>(EventQueue queue, T message, CancellationToken cancellationToken) where T : class;
}
