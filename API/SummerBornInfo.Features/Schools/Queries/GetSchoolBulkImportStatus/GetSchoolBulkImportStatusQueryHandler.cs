namespace SummerBornInfo.Features.Schools.Queries.GetSchoolBulkImportStatus;

public sealed class GetSchoolBulkImportStatusQueryHandler(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<GetSchoolBulkImportStatusResponse?> ExecuteAsync(GetSchoolBulkImportStatusQuery request, CancellationToken cancellationToken)
    {
        var importRequest = await _context.SchoolBulkImportRequests
            .AsNoTracking()
            .Include(x => x.Failures)
            .SingleOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken);

        if (importRequest is null)
        {
            return null;
        }

        return new GetSchoolBulkImportStatusResponse
        {
            SchoolBulkImportRequestId = importRequest.Id,
            Status = importRequest.Status.ToString(),
            LinesProcessed = importRequest.LinesProcessed,
            Failures = importRequest.Failures
                .OrderBy(x => x.LineNumber)
                .Select(x => new SchoolBulkImportFailureResponse
                {
                    LineNumber = x.LineNumber,
                    Message = x.ErrorMessage
                })
                .ToList()
        };
    }
}
