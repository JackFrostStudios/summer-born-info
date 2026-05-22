namespace SummerBornInfo.Web.Authentication;

internal static class ApiCookieAuthenticationEvents
{
    public static Task RedirectToUnauthorizedAsync(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public static Task RedirectToForbiddenAsync(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
