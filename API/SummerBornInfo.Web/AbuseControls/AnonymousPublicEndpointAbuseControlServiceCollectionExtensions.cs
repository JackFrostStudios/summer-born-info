namespace SummerBornInfo.Web.AbuseControls;

internal static class AnonymousPublicEndpointAbuseControlServiceCollectionExtensions
{
    public static IServiceCollection AddAnonymousPublicEndpointAbuseControls(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ConfigureOptions(services, configuration, environment);
        _ = services.AddRateLimiter(ConfigureRateLimiter);
        ConfigureBotVerification(services);

        return services;
    }

    private static void ConfigureOptions(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _ = services
            .AddOptions<AnonymousPublicEndpointAbuseControlOptions>()
            .Bind(configuration.GetSection(AnonymousPublicEndpointAbuseControlOptions.SectionName))
            .Validate(
                HasValidRateLimitingConfiguration,
                "AbuseControls:RateLimiting values must be positive, with queue limits of zero or greater.")
            .Validate(
                HasRequiredTurnstileSecret,
                "AbuseControls:BotVerification:Turnstile:SecretKey is required when Turnstile mode is enabled.")
            .Validate(
                HasRequiredFakeAcceptedToken,
                "AbuseControls:BotVerification:Fake:AcceptedToken is required when fake mode is enabled.")
            .Validate(
                options => HasProductionSafeBotVerificationMode(options, environment),
                "Production requires Turnstile bot verification mode.")
            .ValidateOnStart();
    }

    private static void ConfigureRateLimiter(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = WriteRateLimitResponseAsync;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(CreateRateLimitPartition);
    }

    private static void ConfigureBotVerification(IServiceCollection services)
    {
        _ = services.AddHttpClient<TurnstileAnonymousBotVerifier>(client =>
        {
            client.BaseAddress = new Uri("https://challenges.cloudflare.com/");
        });

        _ = services.AddScoped<IAnonymousBotVerifier>(CreateAnonymousBotVerifier);
    }

    private static bool HasValidRateLimitingConfiguration(AnonymousPublicEndpointAbuseControlOptions options)
    {
        return options.RateLimiting.ReviewSubmission.PermitLimit > 0
            && options.RateLimiting.ReviewSubmission.WindowSeconds > 0
            && options.RateLimiting.ReviewSubmission.QueueLimit >= 0
            && options.RateLimiting.ReviewReportSubmission.PermitLimit > 0
            && options.RateLimiting.ReviewReportSubmission.WindowSeconds > 0
            && options.RateLimiting.ReviewReportSubmission.QueueLimit >= 0;
    }

    private static bool HasRequiredTurnstileSecret(AnonymousPublicEndpointAbuseControlOptions options)
    {
        return options.BotVerification.Mode != AnonymousBotVerificationMode.Turnstile
            || !string.IsNullOrWhiteSpace(options.BotVerification.Turnstile.SecretKey);
    }

    private static bool HasRequiredFakeAcceptedToken(AnonymousPublicEndpointAbuseControlOptions options)
    {
        return options.BotVerification.Mode != AnonymousBotVerificationMode.Fake
            || !string.IsNullOrWhiteSpace(options.BotVerification.Fake.AcceptedToken);
    }

    private static bool HasProductionSafeBotVerificationMode(
        AnonymousPublicEndpointAbuseControlOptions options,
        IHostEnvironment environment)
    {
        return !environment.IsProduction()
            || options.BotVerification.Mode == AnonymousBotVerificationMode.Turnstile;
    }

    private static async ValueTask WriteRateLimitResponseAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        context.HttpContext.Response.ContentType = "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests.",
                Detail = "The request was rejected because the anonymous submission rate limit was exceeded.",
            },
            cancellationToken);
    }

    private static RateLimitPartition<string> CreateRateLimitPartition(HttpContext httpContext)
    {
        var abuseControlOptions = httpContext.RequestServices
            .GetRequiredService<IOptions<AnonymousPublicEndpointAbuseControlOptions>>()
            .Value;

        if (!TryGetAnonymousPublicRouteRateLimitPolicy(httpContext, abuseControlOptions, out var routeKey, out var policyOptions))
        {
            return RateLimitPartition.GetNoLimiter("not-rate-limited");
        }

        return CreateAnonymousFixedWindowLimiter(httpContext, routeKey, policyOptions);
    }

    private static IAnonymousBotVerifier CreateAnonymousBotVerifier(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<AnonymousPublicEndpointAbuseControlOptions>>().Value;

        return options.BotVerification.Mode switch
        {
            AnonymousBotVerificationMode.Disabled => new DisabledAnonymousBotVerifier(),
            AnonymousBotVerificationMode.Fake => ActivatorUtilities.CreateInstance<FakeAnonymousBotVerifier>(serviceProvider),
            AnonymousBotVerificationMode.Turnstile => ActivatorUtilities.CreateInstance<TurnstileAnonymousBotVerifier>(serviceProvider),
            _ => throw new UnreachableException(),
        };
    }

    private static RateLimitPartition<string> CreateAnonymousFixedWindowLimiter(
        HttpContext httpContext,
        string routeKey,
        AnonymousPublicEndpointRateLimitPolicyOptions policyOptions)
    {
        var callerSignal = AnonymousCallerSignalProvider.Get(httpContext);
        var partitionKey = callerSignal.GetRateLimitPartitionKey(routeKey);

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = policyOptions.PermitLimit,
                QueueLimit = policyOptions.QueueLimit,
                Window = TimeSpan.FromSeconds(policyOptions.WindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true,
            });
    }

    private static bool TryGetAnonymousPublicRouteRateLimitPolicy(
        HttpContext httpContext,
        AnonymousPublicEndpointAbuseControlOptions abuseControlOptions,
        out string routeKey,
        out AnonymousPublicEndpointRateLimitPolicyOptions policyOptions)
    {
        routeKey = string.Empty;
        policyOptions = default!;

        if (!HttpMethods.IsPost(httpContext.Request.Method))
        {
            return false;
        }

        var segments = httpContext.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments is null)
        {
            return false;
        }

        if (segments.Length == 4
            && string.Equals(segments[0], "api", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[1], "schools", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(segments[2], out _)
            && string.Equals(segments[3], "csa-application-reviews", StringComparison.OrdinalIgnoreCase))
        {
            routeKey = "csa-review-submission";
            policyOptions = abuseControlOptions.RateLimiting.ReviewSubmission;
            return true;
        }

        if (segments.Length == 6
            && string.Equals(segments[0], "api", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[1], "schools", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(segments[2], out _)
            && string.Equals(segments[3], "csa-application-reviews", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(segments[4], out _)
            && string.Equals(segments[5], "reports", StringComparison.OrdinalIgnoreCase))
        {
            routeKey = "csa-review-report-submission";
            policyOptions = abuseControlOptions.RateLimiting.ReviewReportSubmission;
            return true;
        }

        return false;
    }
}
