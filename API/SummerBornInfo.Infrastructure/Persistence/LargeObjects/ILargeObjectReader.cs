namespace SummerBornInfo.Infrastructure.Persistence.LargeObjects;

public interface ILargeObjectReader
{
    Task<Stream?> ReadLargeObjectAsStreamAsync(uint largeObjectId, CancellationToken cancellationToken = default);
}
