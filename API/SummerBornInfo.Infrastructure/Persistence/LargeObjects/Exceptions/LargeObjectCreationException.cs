namespace SummerBornInfo.Infrastructure.Persistence.LargeObjects.Exceptions;

public sealed class LargeObjectCreationException(string message) : InvalidOperationException(message);
