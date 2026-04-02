namespace SummerBornInfo.Infrastructure.Events;

public interface IEventReader
{
    Task<T?> ReadEventAsync<T>(EventQueue queue, int messageReadTimeoutSeconds, CancellationToken cancellationToken) where T : class;
}
