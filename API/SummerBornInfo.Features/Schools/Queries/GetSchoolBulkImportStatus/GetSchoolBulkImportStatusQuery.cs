namespace SummerBornInfo.Features.Schools.Queries.GetSchoolBulkImportStatus;

public sealed record GetSchoolBulkImportStatusQuery(Guid RequestId);

public sealed record GetSchoolBulkImportStatusResponse
{
    public required Guid SchoolBulkImportRequestId { get; init; }
    public required string Status { get; init; }
    public required int LinesProcessed { get; init; }
    public required IReadOnlyList<SchoolBulkImportFailureResponse> Failures { get; init; }
}

public sealed record SchoolBulkImportFailureResponse
{
    public required int LineNumber { get; init; }
    public required string Message { get; init; }
}
