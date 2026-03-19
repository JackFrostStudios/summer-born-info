namespace SummerBornInfo.Features.Schools.Commands.Import;

public sealed record ImportSchoolsCommand(byte[] Content);

public sealed record ImportSchoolsResponse(
    Guid SchoolBulkImportRequestId
);