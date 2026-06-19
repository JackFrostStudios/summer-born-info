namespace SummerBornInfo.Web.AbuseControls;

internal static class AnonymousCallerSignalProvider
{
    public static AnonymousCallerSignal Get(HttpContext httpContext)
    {
        var cloudflareConnectingIp = httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var remoteIpAddress = !string.IsNullOrWhiteSpace(cloudflareConnectingIp)
            ? cloudflareConnectingIp.Trim()
            : !string.IsNullOrWhiteSpace(forwardedFor)
                ? forwardedFor.Split(',')[0].Trim()
                : httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString().Trim();

        return new AnonymousCallerSignal(
            string.IsNullOrWhiteSpace(remoteIpAddress)
                ? null
                : remoteIpAddress,
            string.IsNullOrWhiteSpace(userAgent)
                ? null
                : userAgent);
    }
}
