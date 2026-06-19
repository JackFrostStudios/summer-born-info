namespace SummerBornInfo.Web.AbuseControls;

internal sealed class AnonymousPublicEndpointRateLimitPolicyOptions
{
    public int PermitLimit { get; init; } = 5;

    public int QueueLimit { get; init; }

    public int WindowSeconds { get; init; } = 60;
}
