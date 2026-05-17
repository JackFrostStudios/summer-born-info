namespace SummerBornInfo.Web.API.Schools;

public static class SchoolEndpoints
{
    public static void RegisterSchoolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var schools = endpoints.MapGroup("/api/schools");

        _ = schools.MapGetAllSchools()
            .MapCreateSchoolsImportRequest()
            .MapGetSchoolsImportRequest();
    }

    private static RouteGroupBuilder MapGetAllSchools(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/", async (GetAllSchoolsQueryHandler handler, Guid? cursor, int? pageSize, CancellationToken cancellationToken) =>
        {
            GetAllSchoolsQuery query = new(cursor, pageSize ?? 100);
            (var schools, var nextCursor) = await handler.ExecuteAsync(query, cancellationToken);
            return Results.Ok(new { schools, nextCursor });
        });

        return builder;
    }

    private static RouteGroupBuilder MapCreateSchoolsImportRequest(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost("/import", async (HttpRequest req, Stream csvFile, ImportSchoolsCommandHandler handler, CancellationToken cancellationToken) =>
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

            ImportSchoolsCommand command = new(csvFile);
            var result = await handler.ExecuteAsync(command, cancellationToken);
            return Results.Ok(result);
        })
            .WithMetadata(new RequestSizeLimitAttribute(100000 * 1024));
        return builder;
    }

    private static RouteGroupBuilder MapGetSchoolsImportRequest(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/import/{requestId:guid}", async (GetSchoolBulkImportStatusQueryHandler handler, Guid requestId, CancellationToken cancellationToken) =>
        {
            GetSchoolBulkImportStatusQuery query = new(requestId);
            var result = await handler.ExecuteAsync(query, cancellationToken);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
        });
        return builder;
    }

}
