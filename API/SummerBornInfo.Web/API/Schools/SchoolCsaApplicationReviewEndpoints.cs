namespace SummerBornInfo.Web.API.Schools;

public static class SchoolCsaApplicationReviewEndpoints
{
    private const string InvalidReviewRequestTitle = "Invalid CSA application review request.";
    private const string InvalidReviewSubmissionTitle = "Invalid CSA application review submission request.";
    private const string InvalidReviewReportTitle = "Invalid CSA application review report request.";
    private const string SchoolNotFoundTitle = "School not found.";
    private const string ReviewNotFoundTitle = "CSA application review not found.";

    public static RouteGroupBuilder MapPublicCsaApplicationReviewEndpoints(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost("/{schoolId:guid}/csa-application-reviews", SubmitReviewAsync)
            .WithName("SubmitPublicCsaApplicationReview")
            .WithSummary("Submit a public CSA application review for a school.")
            .WithDescription("Creates a publicly visible CSA application review. When anonymous bot verification is enabled, callers should supply botVerificationToken. Anonymous submission may return 429 when the route rate limit is exceeded.")
            .Accepts<SubmitPublicCsaApplicationReviewRequest>("application/json")
            .Produces<SubmitPublicCsaApplicationReviewResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests, "application/problem+json");

        _ = builder.MapGet("/{schoolId:guid}/csa-application-reviews", GetReviewsAsync)
            .WithName("GetPublicCsaApplicationReviews")
            .WithSummary("List publicly visible CSA application reviews for a school.")
            .WithDescription("Returns publicly visible reviews in stable newest-first order with cursor pagination. Hidden moderation states are not exposed in this public response.")
            .Produces<GetPublicCsaApplicationReviewsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json");

        _ = builder.MapPost("/{schoolId:guid}/csa-application-reviews/{reviewId:guid}/reports", SubmitReviewReportAsync)
            .WithName("SubmitPublicCsaApplicationReviewReport")
            .WithSummary("Report a publicly visible CSA application review.")
            .WithDescription("Accepts a public report for a visible review. The first valid report against a newly visible review hides it, while an admin-approved review is hidden again after 10 further distinct reports. When anonymous bot verification is enabled, callers should supply botVerificationToken.")
            .Accepts<SubmitPublicCsaApplicationReviewReportRequest>("application/json")
            .Produces<SubmitPublicCsaApplicationReviewReportResponse>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests, "application/problem+json");

        return builder;
    }

    private static async Task<IResult> SubmitReviewAsync(
        Guid schoolId,
        SubmitPublicCsaApplicationReviewRequest request,
        HttpContext httpContext,
        SubmitPublicCsaApplicationReviewCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var callerSignal = AnonymousCallerSignalProvider.Get(httpContext);

        if (!SubmitPublicCsaApplicationReviewCommandValidator.TryValidate(
                schoolId,
                request.Name,
                request.ApplicationSuccessful,
                request.Comment,
                request.BotVerificationToken,
                callerSignal.RemoteIpAddress,
                out var command,
                out var errors))
        {
            return Results.ValidationProblem(
                errors,
                title: InvalidReviewSubmissionTitle);
        }

        var result = await handler.ExecuteAsync(command, cancellationToken);
        return result.Status switch
        {
            SubmitPublicCsaApplicationReviewExecutionStatus.Created => Results.Created(
                $"/api/schools/{schoolId}/csa-application-reviews/{result.Response!.Id}",
                result.Response),
            SubmitPublicCsaApplicationReviewExecutionStatus.SchoolNotFound => CreateSchoolNotFound(
                "No school was found for the supplied school id."),
            SubmitPublicCsaApplicationReviewExecutionStatus.BotVerificationFailed => CreateBotVerificationFailure(
                InvalidReviewSubmissionTitle),
            _ => throw new UnreachableException(),
        };
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

    private static async Task<IResult> SubmitReviewReportAsync(
        Guid schoolId,
        Guid reviewId,
        SubmitPublicCsaApplicationReviewReportRequest request,
        HttpContext httpContext,
        SubmitPublicCsaApplicationReviewReportCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var callerSignal = AnonymousCallerSignalProvider.Get(httpContext);

        if (!SubmitPublicCsaApplicationReviewReportCommandValidator.TryValidate(
                schoolId,
                reviewId,
                request.Reason,
                request.Details,
                callerSignal.GetReporterFingerprint(),
                request.BotVerificationToken,
                callerSignal.RemoteIpAddress,
                out var command,
                out var errors))
        {
            return Results.ValidationProblem(errors, title: InvalidReviewReportTitle);
        }

        var result = await handler.ExecuteAsync(command, cancellationToken);
        return result.Status switch
        {
            SubmitPublicCsaApplicationReviewReportExecutionStatus.Accepted => Results.Accepted(
                value: result.Response),
            SubmitPublicCsaApplicationReviewReportExecutionStatus.SchoolNotFound => CreateSchoolNotFound(
                "No school was found for the supplied school id."),
            SubmitPublicCsaApplicationReviewReportExecutionStatus.ReviewNotFound => Results.Problem(
                detail: "No publicly visible CSA application review was found for the supplied school and review ids.",
                statusCode: StatusCodes.Status404NotFound,
                title: ReviewNotFoundTitle),
            SubmitPublicCsaApplicationReviewReportExecutionStatus.BotVerificationFailed => CreateBotVerificationFailure(
                InvalidReviewReportTitle),
            _ => throw new UnreachableException(),
        };
    }

    private static IResult CreateSchoolNotFound(string detail)
    {
        return Results.Problem(
            detail: detail,
            statusCode: StatusCodes.Status404NotFound,
            title: SchoolNotFoundTitle);
    }

    private static IResult CreateBotVerificationFailure(string title)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["botVerificationToken"] = ["Bot verification failed."],
            },
            title: title);
    }

    internal sealed record SubmitPublicCsaApplicationReviewRequest(
        string? Name,
        bool? ApplicationSuccessful,
        string? Comment,
        string? BotVerificationToken);

    internal sealed record SubmitPublicCsaApplicationReviewReportRequest(
        string? Reason,
        string? Details,
        string? BotVerificationToken);
}
