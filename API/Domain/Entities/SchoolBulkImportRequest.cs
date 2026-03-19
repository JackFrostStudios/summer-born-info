namespace SummerBornInfo.Domain.Entities;

public sealed class SchoolBulkImportRequest
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required byte[] Content { get; init; }
}
