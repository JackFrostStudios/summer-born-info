namespace SummerBornInfo.Features.Schools.Queries.GetSchoolBulkImportStatus.Response;

public sealed record SchoolBulkImportFailureResponse
{
    public required int LineNumber { get; init; }
    public required string Message { get; init; }
}
