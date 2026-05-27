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
            HttpContext httpContext,
            SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQueryHandler searchSchoolsHandler,
            SummerBornInfo.Features.Schools.Queries.GetSchoolByUrn.GetSchoolByUrnQueryHandler getSchoolByUrnHandler,
            string? q,
            string? urn,
            Guid? cursor,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            _ = cursor;

            var hasUrn = httpContext.Request.Query.ContainsKey("urn");
            var hasQuery = httpContext.Request.Query.ContainsKey("q");

            if (hasUrn && hasQuery)
            {
                return Results.BadRequest("Specify exactly one of q or urn.");
            }

            if (hasUrn)
            {
                return await GetSchoolByUrnAsync(getSchoolByUrnHandler, urn, cancellationToken);
            }

            return await SearchSchoolsAsync(searchSchoolsHandler, q, pageSize, cancellationToken);
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
        var response = await handler.ExecuteAsync(query, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetSchoolByUrnAsync(
        SummerBornInfo.Features.Schools.Queries.GetSchoolByUrn.GetSchoolByUrnQueryHandler handler,
        string? urn,
        CancellationToken cancellationToken)
    {
        if (!SummerBornInfo.Features.Schools.Queries.GetSchoolByUrn.GetSchoolByUrnQueryValidator.TryValidate(urn, out var query))
        {
            return Results.BadRequest("URN must be a valid integer.");
        }

        var response = await handler.ExecuteAsync(query, cancellationToken);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> SearchSchoolsAsync(
        SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQueryHandler handler,
        string? q,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQueryValidator.TryValidate(q, pageSize, out var query))
        {
            return Results.BadRequest("q must be at least 4 non-whitespace characters.");
        }

        var response = await handler.ExecuteAsync(query, cancellationToken);
        return Results.Ok(response);
    }
}
