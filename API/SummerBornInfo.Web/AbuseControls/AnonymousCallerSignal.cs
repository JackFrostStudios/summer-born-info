namespace SummerBornInfo.Web.AbuseControls;

internal sealed record AnonymousCallerSignal(
    string? RemoteIpAddress,
    string? UserAgent)
{
    public string GetRateLimitPartitionKey(string routeKey)
    {
        var callerKey = !string.IsNullOrWhiteSpace(RemoteIpAddress)
            ? $"ip:{RemoteIpAddress}"
            : !string.IsNullOrWhiteSpace(UserAgent)
                ? $"ua:{ComputeHash(UserAgent)}"
                : "anonymous";

        return $"{routeKey}|{callerKey}";
    }

    public string? GetReporterFingerprint()
    {
        if (string.IsNullOrWhiteSpace(RemoteIpAddress) && string.IsNullOrWhiteSpace(UserAgent))
        {
            return null;
        }

        return ComputeHash($"{RemoteIpAddress ?? "unknown-ip"}|{UserAgent ?? "unknown-user-agent"}");
    }

    private static string ComputeHash(string value)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hashBytes);
    }
}
