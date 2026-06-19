namespace SummerBornInfo.Web.AbuseControls;

internal sealed class FakeAnonymousBotVerifier(IOptions<AnonymousPublicEndpointAbuseControlOptions> options) : IAnonymousBotVerifier
{
    private readonly IOptions<AnonymousPublicEndpointAbuseControlOptions> _options = options;

    public Task<AnonymousBotVerificationResult> VerifyAsync(
        AnonymousBotVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var acceptedToken = _options.Value.BotVerification.Fake.AcceptedToken;
        var isVerified = string.Equals(request.Token, acceptedToken, StringComparison.Ordinal);

        return Task.FromResult(
            isVerified
                ? AnonymousBotVerificationResult.Verified()
                : AnonymousBotVerificationResult.Failed("Bot verification token is invalid."));
    }
}
