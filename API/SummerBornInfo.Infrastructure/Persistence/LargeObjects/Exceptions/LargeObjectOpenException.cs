namespace SummerBornInfo.Infrastructure.Persistence.LargeObjects.Exceptions;

public sealed class LargeObjectOpenException(string message) : InvalidOperationException(message);
