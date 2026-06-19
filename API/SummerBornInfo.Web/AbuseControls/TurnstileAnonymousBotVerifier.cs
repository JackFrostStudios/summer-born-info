namespace SummerBornInfo.Web.AbuseControls;

internal sealed partial class TurnstileAnonymousBotVerifier(
    HttpClient httpClient,
    IOptions<AnonymousPublicEndpointAbuseControlOptions> options,
    ILogger<TurnstileAnonymousBotVerifier> logger) : IAnonymousBotVerifier
{
    private static readonly Uri SiteVerifyUri = new("turnstile/v0/siteverify", UriKind.Relative);

    private readonly HttpClient _httpClient = httpClient;
    private readonly IOptions<AnonymousPublicEndpointAbuseControlOptions> _options = options;
    private readonly ILogger<TurnstileAnonymousBotVerifier> _logger = logger;

    public async Task<AnonymousBotVerificationResult> VerifyAsync(
        AnonymousBotVerificationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return AnonymousBotVerificationResult.Failed("Bot verification token is required.");
        }

        var verificationOptions = _options.Value.BotVerification;
        var content = new List<KeyValuePair<string, string>>
        {
            new("secret", verificationOptions.Turnstile.SecretKey!),
            new("response", request.Token.Trim()),
        };

        if (!string.IsNullOrWhiteSpace(request.RemoteIpAddress))
        {
            content.Add(new KeyValuePair<string, string>("remoteip", request.RemoteIpAddress.Trim()));
        }

        try
        {
            using var response = await _httpClient.PostAsync(
                SiteVerifyUri,
                new FormUrlEncodedContent(content),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                LogTurnstileHttpFailure(_logger, response.StatusCode);

                return AnonymousBotVerificationResult.Failed("Bot verification could not be completed.");
            }

            var payload = await response.Content.ReadFromJsonAsync<TurnstileSiteVerifyResponse>(cancellationToken);

            if (payload?.Success == true)
            {
                return AnonymousBotVerificationResult.Verified();
            }

            LogTurnstileRejected(_logger, payload?.ErrorCodes is null ? string.Empty : string.Join(',', payload.ErrorCodes));

            return AnonymousBotVerificationResult.Failed("Bot verification failed.");
        }
        catch (Exception exception)
        {
            LogTurnstileRequestException(_logger, exception);
            return AnonymousBotVerificationResult.Failed("Bot verification could not be completed.");
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Turnstile verification failed with HTTP status code {StatusCode}.")]
    private static partial void LogTurnstileHttpFailure(ILogger logger, HttpStatusCode statusCode);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Turnstile rejected anonymous request with error codes: {ErrorCodes}.")]
    private static partial void LogTurnstileRejected(ILogger logger, string errorCodes);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Turnstile verification request threw an exception.")]
    private static partial void LogTurnstileRequestException(ILogger logger, Exception exception);

    private sealed record TurnstileSiteVerifyResponse(
        bool Success,
        [property: JsonPropertyName("error-codes")] IReadOnlyList<string>? ErrorCodes);
}
