namespace SummerBornInfo.Domain.Entities;

public sealed class SchoolBulkImportFailure
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid SchoolBulkImportRequestId { get; set; }
    public int LineNumber { get; init; }
    public required string ErrorMessage { get; init; }
}
