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
        _ = builder.MapPost("/{schoolId:guid}/csa-application-reviews", SubmitReviewAsync);
        _ = builder.MapGet("/{schoolId:guid}/csa-application-reviews", GetReviewsAsync);
        _ = builder.MapPost("/{schoolId:guid}/csa-application-reviews/{reviewId:guid}/reports", SubmitReviewReportAsync);

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
