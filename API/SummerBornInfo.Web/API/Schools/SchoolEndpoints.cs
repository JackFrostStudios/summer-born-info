namespace SummerBornInfo.Web.API.Schools;

public static class SchoolEndpoints
{
    public static void RegisterSchoolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var schools = endpoints.MapGroup("/api/schools");

        _ = schools.MapGetAllSchools()
            .MapGetSchoolsImportRequest();
    }

    private static RouteGroupBuilder MapGetAllSchools(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/", async (GetAllSchoolsQueryHandler handler, Guid? cursor, int? pageSize, CancellationToken cancellationToken) =>
        {
            GetAllSchoolsQuery query = new(cursor, pageSize);
            (var schools, var nextCursor) = await handler.ExecuteAsync(query, cancellationToken);
            return Results.Ok(new { schools, nextCursor });
        });

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
