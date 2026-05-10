namespace SummerBornInfo.Infrastructure.Events;

public sealed record QueuedEvent<T>(long MessageId, T Message) where T : class;
