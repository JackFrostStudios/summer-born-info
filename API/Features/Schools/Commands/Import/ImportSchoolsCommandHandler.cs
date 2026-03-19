namespace SummerBornInfo.Features.Schools.Commands.Import;

public sealed class ImportSchoolsCommandHandler(ApplicationDbContext context)
{
    public async Task<ImportSchoolsResponse> ExecuteAsync(ImportSchoolsCommand command, CancellationToken cancellationToken)
    {
        var schoolBulkImportRequest = new SchoolBulkImportRequest() { Content = command.Content };
        context.Add(schoolBulkImportRequest);
        await context.SaveChangesAsync(cancellationToken);
        return new ImportSchoolsResponse(schoolBulkImportRequest.Id);
    }
}