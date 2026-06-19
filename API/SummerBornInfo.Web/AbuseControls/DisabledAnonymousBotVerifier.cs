namespace SummerBornInfo.Web.AbuseControls;

internal sealed class DisabledAnonymousBotVerifier : IAnonymousBotVerifier
{
    public Task<AnonymousBotVerificationResult> VerifyAsync(
        AnonymousBotVerificationRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(AnonymousBotVerificationResult.Verified());
    }
}
