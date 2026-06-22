namespace SummerBornInfo.Web.AbuseControls;

internal sealed class AnonymousPublicEndpointBotVerificationOptions
{
    public AnonymousBotVerificationMode Mode { get; init; } = AnonymousBotVerificationMode.Turnstile;

    public TurnstileBotVerificationOptions Turnstile { get; init; } = new();

    public FakeBotVerificationOptions Fake { get; init; } = new();
}
