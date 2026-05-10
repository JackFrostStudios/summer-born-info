using SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile;

public sealed class ProcessImportFileCommandHandler(
    ApplicationDbContext context,
    ILargeObjectReader largeObjectReader,
    SchoolsImporter<ApplicationDbContext> schoolsImporter)
{
    public async Task ExecuteAsync(ProcessImportFileCommand command, CancellationToken cancellationToken)
    {
        var schoolBulkImportRequest = await context.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleOrDefaultAsync(x => x.Id == command.SchoolBulkImportRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"School bulk import request '{command.SchoolBulkImportRequestId}' was not found.");

        if (schoolBulkImportRequest.Status is SchoolBulkImportStatus.Completed or SchoolBulkImportStatus.CompletedWithFailures or SchoolBulkImportStatus.Processing)
        {
            return;
        }

        await using var csvStream = await largeObjectReader.ReadLargeObjectAsStreamAsync(schoolBulkImportRequest.ContentId, cancellationToken)
            ?? throw new InvalidOperationException($"Large object '{schoolBulkImportRequest.ContentId}' was not found.");

        schoolBulkImportRequest.Status = SchoolBulkImportStatus.Processing;
        await context.SaveChangesAsync(cancellationToken);

        try
        {
            await foreach (var result in schoolsImporter.ImportAsync(csvStream, cancellationToken))
            {
                schoolBulkImportRequest.LinesProcessed++;

                if (!result.Succeeded)
                {
                    schoolBulkImportRequest.Failures.Add(new SchoolBulkImportFailure
                    {
                        LineNumber = result.LineNumber,
                        ErrorMessage = result.ErrorMessage ?? "Unknown import error",
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            schoolBulkImportRequest.Status = schoolBulkImportRequest.Failures.Count == 0
                ? SchoolBulkImportStatus.Completed
                : SchoolBulkImportStatus.CompletedWithFailures;

            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            schoolBulkImportRequest.Status = SchoolBulkImportStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}
