namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

public interface ISchoolsImporter
{
    IAsyncEnumerable<SchoolImportResult> ImportAsync(
        Guid schoolBulkImportRequestId,
        Stream csvStream,
        int processedRowsToSkip = 0,
        CancellationToken cancellationToken = default);
}
