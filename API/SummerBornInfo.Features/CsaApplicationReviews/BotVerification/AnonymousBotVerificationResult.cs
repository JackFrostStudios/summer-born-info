namespace SummerBornInfo.Features.CsaApplicationReviews.BotVerification;

public sealed record AnonymousBotVerificationResult(
    bool IsVerified,
    string? FailureMessage)
{
    public static AnonymousBotVerificationResult Verified()
    {
        return new AnonymousBotVerificationResult(
            IsVerified: true,
            FailureMessage: null);
    }

    public static AnonymousBotVerificationResult Failed(string failureMessage)
    {
        return new AnonymousBotVerificationResult(
            IsVerified: false,
            failureMessage);
    }
}
