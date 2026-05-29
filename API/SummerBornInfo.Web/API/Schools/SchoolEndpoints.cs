namespace SummerBornInfo.Web.API.Schools;

public static class SchoolEndpoints
{
    private const string InvalidDiscoveryRequestTitle = "Invalid school discovery request.";
    private const string SchoolNotFoundTitle = "School not found.";

    public static void RegisterSchoolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var schools = endpoints.MapGroup("/api/schools");

        _ = schools.MapGetAllSchools()
            .MapGetNearbySchools()
            .MapSearchSchools();
    }

    private static RouteGroupBuilder MapGetAllSchools(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/", GetSchoolsAsync)
            .AddSchoolCollectionOpenApiMetadata();

        return builder;
    }

    private static RouteGroupBuilder MapSearchSchools(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/search", async (
            HttpContext httpContext,
            SearchSchoolsQueryHandler searchSchoolsHandler,
            GetSchoolByUrnQueryHandler getSchoolByUrnHandler,
            string? q,
            string? urn,
            string? cursor,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var hasUrn = httpContext.Request.Query.ContainsKey("urn");
            var hasQuery = httpContext.Request.Query.ContainsKey("q");

            if (hasUrn && hasQuery)
            {
                return CreateInvalidDiscoveryRequest(
                    "Specify exactly one of q or urn.");
            }

            if (hasUrn && (cursor is not null || pageSize is not null))
            {
                return CreateInvalidDiscoveryRequest(
                    "cursor and pageSize are not supported when looking up a school by urn.");
            }

            if (hasUrn)
            {
                return await GetSchoolByUrnAsync(getSchoolByUrnHandler, urn, cancellationToken);
            }

            return await SearchSchoolsAsync(searchSchoolsHandler, q, cursor, pageSize, cancellationToken);
        })
            .AddSchoolSearchOpenApiMetadata();

        return builder;
    }

    private static RouteGroupBuilder MapGetNearbySchools(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/nearby", GetNearbySchoolsAsync)
            .AddNearbySchoolOpenApiMetadata();

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
        GetSchoolByUrnQueryHandler handler,
        string? urn,
        CancellationToken cancellationToken)
    {
        if (!GetSchoolByUrnQueryValidator.TryValidate(urn, out var query))
        {
            return CreateInvalidDiscoveryRequest("URN must be a valid integer.");
        }

        var response = await handler.ExecuteAsync(query, cancellationToken);
        return response is null
            ? CreateSchoolNotFound("No school was found for the supplied URN.")
            : Results.Ok(response);
    }

    private static async Task<IResult> SearchSchoolsAsync(
        SearchSchoolsQueryHandler handler,
        string? q,
        string? cursor,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!SearchSchoolsQueryValidator.TryValidate(q, cursor, pageSize, out var query))
        {
            return CreateInvalidDiscoveryRequest(
                "q must be at least 4 non-whitespace characters, pageSize must be between 1 and 200, and cursor must be a valid search continuation token.");
        }

        var response = await handler.ExecuteAsync(query, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetNearbySchoolsAsync(
        SummerBornInfo.Features.Schools.Queries.GetNearbySchools.GetNearbySchoolsQueryHandler handler,
        [AsParameters] SummerBornInfo.Features.Schools.Queries.GetNearbySchools.GetNearbySchoolsRequest request,
        CancellationToken cancellationToken)
    {
        if (!SummerBornInfo.Features.Schools.Queries.GetNearbySchools.GetNearbySchoolsRequestValidator.TryValidate(request, out var query))
        {
            return CreateInvalidDiscoveryRequest(
                "latitude must be between -90 and 90, longitude must be between -180 and 180, radiusMiles must be greater than 0 and no more than 100, pageSize must be between 1 and 200, and cursor must be a valid nearby search continuation token.");
        }

        var response = await handler.ExecuteAsync(query, cancellationToken);
        return Results.Ok(response);
    }

    private static IResult CreateInvalidDiscoveryRequest(string detail)
    {
        return Results.Problem(
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            title: InvalidDiscoveryRequestTitle);
    }

    private static IResult CreateSchoolNotFound(string detail)
    {
        return Results.Problem(
            detail: detail,
            statusCode: StatusCodes.Status404NotFound,
            title: SchoolNotFoundTitle);
    }
}
