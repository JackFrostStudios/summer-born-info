namespace SummerBornInfo.Web.API.Schools;

public static class SchoolEndpoints
{
    public static void RegisterSchoolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var schools = endpoints.MapGroup("/api/schools");

        schools.MapPost("/import", async (HttpRequest req, Stream csvFile, ImportSchoolsCommandHandler handler) =>
        {
            var maxMessageSize = 100000 * 1024;
            if (req.ContentLength is not null && req.ContentLength > maxMessageSize)
            {
                return Results.BadRequest("CSV file is too large.");
            }
            if (csvFile == null || req.ContentLength == 0)
            {
                return Results.BadRequest("CSV file is required");
            }

            var command = new ImportSchoolsCommand(csvFile);
            var result = await handler.ExecuteAsync(command, CancellationToken.None);
            return Results.Ok(result);
        })
            .WithMetadata(new RequestSizeLimitAttribute(100000 * 1024));

        schools.MapGet("/", async (GetAllSchoolsQueryHandler handler, Guid? cursor, int? pageSize) =>
        {
            var query = new GetAllSchoolsQuery(cursor, pageSize ?? 100);
            var (schools, nextCursor) = await handler.ExecuteAsync(query, CancellationToken.None);
            return Results.Ok(new { schools, nextCursor });
        });

        schools.MapGet("/import/{requestId:guid}", async (GetSchoolBulkImportStatusQueryHandler handler, Guid requestId) =>
        {
            var query = new GetSchoolBulkImportStatusQuery(requestId);
            var result = await handler.ExecuteAsync(query, CancellationToken.None);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
        });
    }

}
