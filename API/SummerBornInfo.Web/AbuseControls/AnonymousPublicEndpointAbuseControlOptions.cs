namespace SummerBornInfo.Web.AbuseControls;

internal sealed class AnonymousPublicEndpointAbuseControlOptions
{
    public const string SectionName = "AbuseControls";

    public AnonymousPublicEndpointRateLimitingOptions RateLimiting { get; init; } = new();

    public AnonymousPublicEndpointBotVerificationOptions BotVerification { get; init; } = new();
}
