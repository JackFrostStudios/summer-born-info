namespace SummerBornInfo.Features.Tests.TestFramework;

internal sealed class AlwaysVerifiedAnonymousBotVerifier : IAnonymousBotVerifier
{
    public Task<AnonymousBotVerificationResult> VerifyAsync(
        AnonymousBotVerificationRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(AnonymousBotVerificationResult.Verified());
    }
}
