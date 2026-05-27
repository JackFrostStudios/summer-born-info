namespace SummerBornInfo.Web.API.Schools;

public static class SchoolEndpoints
{
    public static void RegisterSchoolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var schools = endpoints.MapGroup("/api/schools");

        _ = schools.MapGetAllSchools()
            .MapSearchSchools();
    }

    private static RouteGroupBuilder MapGetAllSchools(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/", GetSchoolsAsync);

        return builder;
    }

    private static RouteGroupBuilder MapSearchSchools(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/search", async (
            GetAllSchoolsQueryHandler handler,
            string? q,
            int? urn,
            Guid? cursor,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            _ = q;
            _ = urn;

            return await GetSchoolsAsync(handler, cursor, pageSize, cancellationToken);
        });

        return builder;
    }

    private static async Task<IResult> GetSchoolsAsync(
        GetAllSchoolsQueryHandler handler,
        Guid? cursor,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        GetAllSchoolsQuery query = new(cursor, pageSize);
        (var schools, var nextCursor) = await handler.ExecuteAsync(query, cancellationToken);
        return Results.Ok(new { schools, nextCursor });
    }
}
