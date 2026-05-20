namespace SummerBornInfo.Features.Schools.Queries.GetSchoolBulkImportStatus.Response;

public sealed record GetSchoolBulkImportStatusResponse
{
    public required Guid SchoolBulkImportRequestId { get; init; }
    public required string Status { get; init; }
    public required int LinesProcessed { get; init; }
    public required IReadOnlyList<SchoolBulkImportFailureResponse> Failures { get; init; }
}
