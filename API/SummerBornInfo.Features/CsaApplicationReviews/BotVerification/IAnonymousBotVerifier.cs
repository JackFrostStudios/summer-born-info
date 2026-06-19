namespace SummerBornInfo.Features.CsaApplicationReviews.BotVerification;

public interface IAnonymousBotVerifier
{
    Task<AnonymousBotVerificationResult> VerifyAsync(
        AnonymousBotVerificationRequest request,
        CancellationToken cancellationToken);
}
