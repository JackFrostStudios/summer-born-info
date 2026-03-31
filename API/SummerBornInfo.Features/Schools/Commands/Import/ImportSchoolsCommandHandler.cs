using SummerBornInfo.Domain.Events;
using SummerBornInfo.Infrastructure.Events;
using SummerBornInfo.Infrastructure.Persistence.LargeObjects;

namespace SummerBornInfo.Features.Schools.Commands.Import;

public sealed class ImportSchoolsCommandHandler(ApplicationDbContext context, ILargeObjectWriter largeObjectWriter, IEventEmitter eventEmitter)
{
    public async Task<ImportSchoolsResponse> ExecuteAsync(ImportSchoolsCommand command, CancellationToken cancellationToken)
    {
        await using var efTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        
        var largeObjectId = await largeObjectWriter.StreamContentToNewLargeObjectAsync(command.Content, cancellationToken);

        var schoolBulkImportRequest = await SaveNewSchoolBulkImportRequestAsync(largeObjectId, cancellationToken);

        await EmitSchoolBulkImportEvent(schoolBulkImportRequest, cancellationToken);

        await efTransaction.CommitAsync(cancellationToken);

        return new ImportSchoolsResponse(schoolBulkImportRequest.Id);
    }

    private async Task<SchoolBulkImportRequest> SaveNewSchoolBulkImportRequestAsync(uint largeObjectId, CancellationToken cancellationToken)
    {
        var schoolBulkImportRequest = new SchoolBulkImportRequest() { ContentId = largeObjectId };
        context.Add(schoolBulkImportRequest);
        await context.SaveChangesAsync(cancellationToken);
        return schoolBulkImportRequest;
    }

    private async Task EmitSchoolBulkImportEvent(SchoolBulkImportRequest schoolBulkImportRequest, CancellationToken cancellationToken)
    {
        var schoolBulkImportUploadedEvent = new SchoolBulkImportUploaded
        {
            SchoolBulkImportRequestId = schoolBulkImportRequest.Id
        };

        await eventEmitter.EmitEventAsync(EventQueue.SchoolBulkImport, schoolBulkImportUploadedEvent, cancellationToken);
    }
}