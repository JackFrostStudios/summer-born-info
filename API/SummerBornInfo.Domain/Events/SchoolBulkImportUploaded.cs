namespace SummerBornInfo.Domain.Events;

public sealed record SchoolBulkImportUploaded
{
    public required Guid SchoolBulkImportRequestId { get; init; }
}
