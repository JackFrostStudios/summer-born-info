namespace SummerBornInfo.Web.API.Admin.SchoolImports;

public static class AdminSchoolImportEndpoints
{
    private const int MaxMessageSizeBytes = 100000 * 1024;

    public static void RegisterAdminSchoolImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var schoolImports = endpoints.MapGroup("/api/admin/school-imports")
            .RequireAuthorization(AdminAuthorizationPolicyNames.Admin);

        _ = schoolImports.MapCreateSchoolImportRequest()
            .MapGetSchoolImportStatus();
    }

    private static RouteGroupBuilder MapCreateSchoolImportRequest(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost(string.Empty, async (HttpRequest req, Stream csvFile, ImportSchoolsCommandHandler handler, CancellationToken cancellationToken) =>
        {
            if (req.ContentLength is > MaxMessageSizeBytes)
            {
                return Results.BadRequest("CSV file is too large.");
            }

            if (req.ContentLength == 0)
            {
                return Results.BadRequest("CSV file is required");
            }

            ImportSchoolsCommand command = new(csvFile);
            var result = await handler.ExecuteAsync(command, cancellationToken);
            return Results.Accepted(uri: null, value: result);
        })
            .WithMetadata(new RequestSizeLimitAttribute(MaxMessageSizeBytes));

        return builder;
    }

    private static RouteGroupBuilder MapGetSchoolImportStatus(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet("/{requestId:guid}", async (GetSchoolBulkImportStatusQueryHandler handler, Guid requestId, CancellationToken cancellationToken) =>
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
