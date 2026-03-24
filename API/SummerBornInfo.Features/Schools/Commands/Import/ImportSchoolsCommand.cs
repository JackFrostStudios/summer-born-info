namespace SummerBornInfo.Features.Schools.Commands.Import;

public sealed record ImportSchoolsCommand(Stream Content);

public sealed record ImportSchoolsResponse(
    Guid SchoolBulkImportRequestId
);