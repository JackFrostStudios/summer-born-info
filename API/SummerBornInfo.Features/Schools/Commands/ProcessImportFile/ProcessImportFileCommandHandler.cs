namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile;

public sealed class ProcessImportFileCommandHandler(
    ApplicationDbContext context,
    ILargeObjectReader largeObjectReader,
    ISchoolsImporter schoolsImporter) : IProcessImportFileCommandHandler
{
    public async Task ExecuteAsync(ProcessImportFileCommand command, CancellationToken cancellationToken)
    {
        var schoolBulkImportRequest = await context.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleOrDefaultAsync(x => x.Id == command.SchoolBulkImportRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"School bulk import request '{command.SchoolBulkImportRequestId}' was not found.");

        if (!schoolBulkImportRequest.ProcessingStarted())
        {
            return;
        }

        await using var csvStream = await largeObjectReader.ReadLargeObjectAsStreamAsync(schoolBulkImportRequest.ContentId, cancellationToken)
            ?? throw new InvalidOperationException($"Large object '{schoolBulkImportRequest.ContentId}' was not found.");

        _ = await context.SaveChangesAsync(cancellationToken);

        try
        {
            await foreach (var result in schoolsImporter.ImportAsync(
                command.SchoolBulkImportRequestId,
                csvStream,
                schoolBulkImportRequest.LinesProcessed,
                cancellationToken))
            {
                schoolBulkImportRequest.UpdateProgress(result.LineNumber, result.Succeeded ? null : result.ErrorMessage ?? "Unknown import error");

                _ = await context.SaveChangesAsync(cancellationToken);
            }

            schoolBulkImportRequest.ProcessingComplete();
            _ = await context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            schoolBulkImportRequest.ProcessingFailed();
            _ = await context.SaveChangesAsync(cancellationToken);
            throw ex is SchoolBulkImportException
                ? ex
                : new SchoolBulkImportProcessingException(ex);
        }
    }
}
