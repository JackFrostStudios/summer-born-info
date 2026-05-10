namespace SummerBornInfo.Domain.Entities;

public sealed class SchoolBulkImportRequest
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required uint ContentId { get; init; }
    public int LinesProcessed { get; set; }
    public SchoolBulkImportStatus Status { get; set; } = SchoolBulkImportStatus.Pending;
    public List<SchoolBulkImportFailure> Failures { get; init; } = [];
}
