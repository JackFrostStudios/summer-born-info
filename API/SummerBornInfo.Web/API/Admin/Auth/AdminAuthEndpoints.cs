namespace SummerBornInfo.Web.API.Admin.Auth;

public static class AdminAuthEndpoints
{
    public static void RegisterAdminAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminAuth = endpoints.MapGroup("/api/admin/auth");

        _ = adminAuth.MapSignIn()
            .MapSignOut();
    }

    private static RouteGroupBuilder MapSignIn(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost("/sign-in", HandleSignInAsync)
            .AllowAnonymous();

        return builder;
    }

    private static RouteGroupBuilder MapSignOut(this RouteGroupBuilder builder)
    {
        _ = builder.MapPost(
                "/sign-out",
                async (SignInManager<ApplicationUser> signInManager) =>
                {
                    await signInManager.SignOutAsync();
                    return Results.NoContent();
                })
            .AllowAnonymous();

        return builder;
    }

    private static async Task<IResult> HandleSignInAsync(
        HttpContext httpContext,
        AdminSignInRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true
            && !httpContext.User.IsInRole(ApplicationRoleNames.Admin))
        {
            return Results.Forbid();
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var passwordSignInResult = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: false);
        if (!passwordSignInResult.Succeeded)
        {
            return Results.Unauthorized();
        }

        if (!await userManager.IsInRoleAsync(user, ApplicationRoleNames.Admin))
        {
            return Results.Forbid();
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.NoContent();
    }

    internal sealed record AdminSignInRequest(string Email, string Password);
}
