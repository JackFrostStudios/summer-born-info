namespace SummerBornInfo.Infrastructure.Events;

public sealed record QueuedEvent<T>(long MessageId, T Message, long ReadCount) where T : class;
