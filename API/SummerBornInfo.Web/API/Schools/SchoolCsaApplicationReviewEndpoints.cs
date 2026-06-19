namespace SummerBornInfo.Web.API.Schools;

public static class SchoolCsaApplicationReviewEndpoints
{
    private const string InvalidReviewRequestTitle = "Invalid CSA application review request.";
    private const string InvalidReviewSubmissionTitle = "Invalid CSA application review submission request.";
    private const string SchoolNotFoundTitle = "School not found.";

    public static RouteGroupBuilder MapPublicCsaApplicationReviewEndpoints(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost("/{schoolId:guid}/csa-application-reviews", SubmitReviewAsync);
        _ = builder.MapGet("/{schoolId:guid}/csa-application-reviews", GetReviewsAsync);

        return builder;
    }

    private static async Task<IResult> SubmitReviewAsync(
        Guid schoolId,
        SubmitPublicCsaApplicationReviewRequest request,
        SubmitPublicCsaApplicationReviewCommandHandler handler,
        CancellationToken cancellationToken)
    {
        if (!SubmitPublicCsaApplicationReviewCommandValidator.TryValidate(
                schoolId,
                request.Name,
                request.ApplicationSuccessful,
                request.Comment,
                out var command,
                out var errors))
        {
            return Results.ValidationProblem(
                errors,
                title: InvalidReviewSubmissionTitle);
        }

        var response = await handler.ExecuteAsync(command, cancellationToken);
        return response is null
            ? CreateSchoolNotFound("No school was found for the supplied school id.")
            : Results.Created($"/api/schools/{schoolId}/csa-application-reviews/{response.Id}", response);
    }

    private static async Task<IResult> GetReviewsAsync(
        Guid schoolId,
        GetPublicCsaApplicationReviewsQueryHandler handler,
        string? cursor,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!GetPublicCsaApplicationReviewsQueryValidator.TryValidate(schoolId, cursor, pageSize, out var query))
        {
            return Results.Problem(
                detail: "pageSize must be between 1 and 200, and cursor must be a valid review continuation token.",
                statusCode: StatusCodes.Status400BadRequest,
                title: InvalidReviewRequestTitle);
        }

        var response = await handler.ExecuteAsync(query, cancellationToken);
        return response is null
            ? CreateSchoolNotFound("No school was found for the supplied school id.")
            : Results.Ok(response);
    }

    private static IResult CreateSchoolNotFound(string detail)
    {
        return Results.Problem(
            detail: detail,
            statusCode: StatusCodes.Status404NotFound,
            title: SchoolNotFoundTitle);
    }

    internal sealed record SubmitPublicCsaApplicationReviewRequest(
        string? Name,
        bool? ApplicationSuccessful,
        string? Comment);
}
