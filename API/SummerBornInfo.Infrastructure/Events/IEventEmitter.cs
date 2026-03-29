namespace SummerBornInfo.Infrastructure.Events;

public interface IEventEmitter
{
    Task EmitEventAsync<T>(T message, CancellationToken cancellationToken) where T : class;
}
