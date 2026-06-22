namespace SummerBornInfo.Web.AbuseControls;

internal sealed class AnonymousPublicEndpointRateLimitingOptions
{
    public AnonymousPublicEndpointRateLimitPolicyOptions ReviewSubmission { get; init; } = new();

    public AnonymousPublicEndpointRateLimitPolicyOptions ReviewReportSubmission { get; init; } = new();
}
