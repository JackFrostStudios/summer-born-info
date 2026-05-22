namespace SummerBornInfo.Web.API.Admin.CsaApplicationReviews;

public static class AdminCsaApplicationReviewModerationEndpoints
{
    public static void RegisterAdminCsaApplicationReviewModerationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var csaApplicationReviews = endpoints.MapGroup("/api/admin/csa-application-reviews")
            .RequireAuthorization(AdminAuthorizationPolicyNames.Admin);

        _ = csaApplicationReviews.MapModeration();
    }

    private static RouteGroupBuilder MapModeration(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost("/{reviewId}/moderation", HandleModerationAsync);

        return builder;
    }

    private static async Task<IResult> HandleModerationAsync(
        string reviewId,
        ModerateCsaApplicationReviewRequest request,
        ModerateCsaApplicationReviewCommandHandler handler,
        CancellationToken cancellationToken)
    {
        ModerateCsaApplicationReviewCommand command = new(
            reviewId,
            request.Decision,
            request.ModeratorNote);

        var response = await handler.ExecuteAsync(command, cancellationToken);
        if (response is null)
        {
            return Results.BadRequest("Decision must be 'approve' or 'reject'.");
        }

        return Results.Ok(response);
    }

    internal sealed record ModerateCsaApplicationReviewRequest(string? Decision, string? ModeratorNote);
}
