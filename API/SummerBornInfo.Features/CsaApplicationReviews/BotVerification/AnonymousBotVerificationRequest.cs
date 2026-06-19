namespace SummerBornInfo.Features.CsaApplicationReviews.BotVerification;

public sealed record AnonymousBotVerificationRequest(
    string? Token,
    string? RemoteIpAddress);
