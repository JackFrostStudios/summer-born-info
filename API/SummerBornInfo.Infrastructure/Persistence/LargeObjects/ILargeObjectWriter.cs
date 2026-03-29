namespace SummerBornInfo.Infrastructure.Persistence.LargeObjects;

public interface ILargeObjectWriter
{
    Task<uint> StreamContentToNewLargeObjectAsync(Stream content, CancellationToken cancellationToken = default);
}
