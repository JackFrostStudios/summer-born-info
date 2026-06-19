namespace SummerBornInfo.Web.API.Admin.CsaApplicationReviews;

public static class AdminCsaApplicationReviewModerationEndpoints
{
    private const string InvalidModerationRequestTitle = "Invalid CSA application review moderation request.";
    private const string InvalidQueueRequestTitle = "Invalid CSA application review moderation queue request.";
    private const string ReviewNotFoundTitle = "CSA application review not found.";

    public static void RegisterAdminCsaApplicationReviewModerationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var csaApplicationReviews = endpoints.MapGroup("/api/admin/csa-application-reviews")
            .RequireAuthorization(AdminAuthorizationPolicyNames.Admin);

        _ = csaApplicationReviews.MapModerationQueue();
        _ = csaApplicationReviews.MapModeration();
    }

    private static RouteGroupBuilder MapModerationQueue(this RouteGroupBuilder builder)
    {
        _ = builder.MapGet(string.Empty, HandleGetQueueAsync)
            .WithName("GetAdminCsaApplicationReviewModerationQueue")
            .WithSummary("List CSA application reviews that require admin moderation.")
            .WithDescription("Returns reviews in the moderation queue, including PendingApproval and PendingReapproval items, with queue filters and cursor pagination for moderator triage.")
            .Produces<GetAdminCsaApplicationReviewQueueResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json");

        return builder;
    }

    private static RouteGroupBuilder MapModeration(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost("/{reviewId:guid}/moderation", HandleModerationAsync)
            .WithName("ModerateCsaApplicationReview")
            .WithSummary("Approve or reject a queued CSA application review.")
            .WithDescription("Accepts an admin moderation decision for a queued review. Approvals restore public visibility, while reapprovals also reset the post-approval distinct report counter.")
            .Accepts<ModerateCsaApplicationReviewRequest>("application/json")
            .Produces<ModerateCsaApplicationReviewResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json");

        return builder;
    }

    private static async Task<IResult> HandleGetQueueAsync(
        [FromQuery(Name = "queueState")] string[]? queueState,
        string? cursor,
        int? pageSize,
        GetAdminCsaApplicationReviewQueueQueryHandler handler,
        CancellationToken cancellationToken)
    {
        if (!GetAdminCsaApplicationReviewQueueQueryValidator.TryValidate(queueState, cursor, pageSize, out var query))
        {
            return Results.Problem(
                detail: "pageSize must be between 1 and 200, queueState must contain only PendingApproval or PendingReapproval values, and cursor must match the current queue filter.",
                statusCode: StatusCodes.Status400BadRequest,
                title: InvalidQueueRequestTitle);
        }

        var response = await handler.ExecuteAsync(query, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> HandleModerationAsync(
        Guid reviewId,
        ModerateCsaApplicationReviewRequest request,
        ModerateCsaApplicationReviewCommandHandler handler,
        CancellationToken cancellationToken)
    {
        ModerateCsaApplicationReviewCommand command = new(
            reviewId,
            request.Decision,
            request.ModeratorNote);

        var result = await handler.ExecuteAsync(command, cancellationToken);
        if (result.Status == ModerateCsaApplicationReviewExecutionStatus.InvalidDecision)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>(StringComparer.Ordinal)
                {
                    ["decision"] = ["Decision must be 'approve' or 'reject'."],
                },
                title: InvalidModerationRequestTitle);
        }

        if (result.Status == ModerateCsaApplicationReviewExecutionStatus.ReviewNotFound)
        {
            return Results.Problem(
                detail: "No CSA application review was found for the supplied review id.",
                statusCode: StatusCodes.Status404NotFound,
                title: ReviewNotFoundTitle);
        }

        if (result.Status == ModerateCsaApplicationReviewExecutionStatus.ReviewNotPending)
        {
            return Results.Problem(
                detail: "Only reviews in the moderation queue can be approved or rejected.",
                statusCode: StatusCodes.Status409Conflict,
                title: InvalidModerationRequestTitle);
        }

        return Results.Ok(result.Response);
    }

    internal sealed record ModerateCsaApplicationReviewRequest(string? Decision, string? ModeratorNote);
}
