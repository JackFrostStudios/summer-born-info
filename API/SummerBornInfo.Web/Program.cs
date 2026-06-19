var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi(options => options.AddAdminSecurityMetadata());
builder.Services.Configure<SchoolBulkImportWorkerOptions>(builder.Configuration.GetSection(SchoolBulkImportWorkerOptions.SectionName));
builder.Services.Configure<DevelopmentAdminBootstrapOptions>(builder.Configuration);
builder.Services
    .AddOptions<AnonymousPublicEndpointAbuseControlOptions>()
    .Bind(builder.Configuration.GetSection(AnonymousPublicEndpointAbuseControlOptions.SectionName))
    .Validate(
        options => options.RateLimiting.ReviewSubmission.PermitLimit > 0
            && options.RateLimiting.ReviewSubmission.WindowSeconds > 0
            && options.RateLimiting.ReviewSubmission.QueueLimit >= 0
            && options.RateLimiting.ReviewReportSubmission.PermitLimit > 0
            && options.RateLimiting.ReviewReportSubmission.WindowSeconds > 0
            && options.RateLimiting.ReviewReportSubmission.QueueLimit >= 0,
        "AbuseControls:RateLimiting values must be positive, with queue limits of zero or greater.")
    .Validate(
        options => options.BotVerification.Mode != AnonymousBotVerificationMode.Turnstile
            || !string.IsNullOrWhiteSpace(options.BotVerification.Turnstile.SecretKey),
        "AbuseControls:BotVerification:Turnstile:SecretKey is required when Turnstile mode is enabled.")
    .Validate(
        options => options.BotVerification.Mode != AnonymousBotVerificationMode.Fake
            || !string.IsNullOrWhiteSpace(options.BotVerification.Fake.AcceptedToken),
        "AbuseControls:BotVerification:Fake:AcceptedToken is required when fake mode is enabled.")
    .Validate(
        options => !builder.Environment.IsProduction()
            || options.BotVerification.Mode == AnonymousBotVerificationMode.Turnstile,
        "Production requires Turnstile bot verification mode.")
    .ValidateOnStart();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
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
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var abuseControlOptions = httpContext.RequestServices
            .GetRequiredService<IOptions<AnonymousPublicEndpointAbuseControlOptions>>()
            .Value;

        if (!TryGetAnonymousPublicRouteRateLimitPolicy(httpContext, abuseControlOptions, out var routeKey, out var policyOptions))
        {
            return RateLimitPartition.GetNoLimiter("not-rate-limited");
        }

        return CreateAnonymousFixedWindowLimiter(httpContext, routeKey, policyOptions);
    });
});

var connectionString = builder.Configuration.GetConnectionString("SummerbornInfo");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ApiCookieAuthenticationEvents.RedirectToUnauthorizedAsync;
    options.Events.OnRedirectToAccessDenied = ApiCookieAuthenticationEvents.RedirectToForbiddenAsync;
});
builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(
        AdminAuthorizationPolicyNames.Admin,
        policy => policy.RequireRole(ApplicationRoleNames.Admin));

builder.Services.AddScoped<IDevelopmentAdminBootstrapper, DevelopmentAdminBootstrapper>();
builder.Services.AddScoped<GetAdminCsaApplicationReviewQueueQueryHandler>();
builder.Services.AddScoped<ModerateCsaApplicationReviewCommandHandler>();
builder.Services.AddScoped<SubmitPublicCsaApplicationReviewCommandHandler>();
builder.Services.AddScoped<SubmitPublicCsaApplicationReviewReportCommandHandler>();
builder.Services.AddScoped<ImportSchoolsCommandHandler>();
builder.Services.AddScoped<IProcessImportFileCommandHandler, ProcessImportFileCommandHandler>();
builder.Services.AddScoped<GetAllSchoolsQueryHandler>();
builder.Services.AddScoped<GetPublicCsaApplicationReviewsQueryHandler>();
builder.Services.AddScoped<GetNearbySchoolsQueryHandler>();
builder.Services.AddScoped<SearchSchoolsQueryHandler>();
builder.Services.AddScoped<GetSchoolByUrnQueryHandler>();
builder.Services.AddScoped<GetSchoolBulkImportStatusQueryHandler>();
builder.Services.AddSingleton<IBritishNationalGridLocationConverter, BritishNationalGridLocationConverter>();
builder.Services.AddScoped<ISchoolsImporter, SchoolsImporter<ApplicationDbContext>>();
builder.Services.AddScoped<ILargeObjectWriter, LargeObjectWriter>();
builder.Services.AddScoped<ILargeObjectReader, LargeObjectReader>();
builder.Services.AddScoped<IEventEmitter, EventEmitter>();
builder.Services.AddScoped<IEventReader, EventReader>();
builder.Services.AddScoped<IEventAcknowledger, EventAcknowledger>();
builder.Services.AddHttpClient<TurnstileAnonymousBotVerifier>(client =>
{
    client.BaseAddress = new Uri("https://challenges.cloudflare.com/");
});
builder.Services.AddScoped<IAnonymousBotVerifier>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AnonymousPublicEndpointAbuseControlOptions>>().Value;

    return options.BotVerification.Mode switch
    {
        AnonymousBotVerificationMode.Disabled => new DisabledAnonymousBotVerifier(),
        AnonymousBotVerificationMode.Fake => ActivatorUtilities.CreateInstance<FakeAnonymousBotVerifier>(serviceProvider),
        AnonymousBotVerificationMode.Turnstile => ActivatorUtilities.CreateInstance<TurnstileAnonymousBotVerifier>(serviceProvider),
        _ => throw new UnreachableException(),
    };
});
builder.Services.AddHostedService<ProcessSchoolBulkImportBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await PostgreSqlDatabaseBootstrapper.EnsureApplicationDatabaseAsync(dbContext, app.Lifetime.ApplicationStopping);
    var developmentAdminBootstrapper = scope.ServiceProvider.GetRequiredService<IDevelopmentAdminBootstrapper>();
    await developmentAdminBootstrapper.UpsertAsync(app.Lifetime.ApplicationStopping);
    NpgmqClient npgmq = new(connection: dbContext.GetNpgsqlConnection());
    await npgmq.InitAsync(app.Lifetime.ApplicationStopping);
    await npgmq.CreateQueueAsync(EventQueue.SchoolBulkImport.Name, app.Lifetime.ApplicationStopping);
    _ = app.MapOpenApi();
    _ = app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.RegisterAdminAuthEndpoints();
app.RegisterAdminCsaApplicationReviewModerationEndpoints();
app.RegisterAdminSchoolImportEndpoints();
app.RegisterSchoolEndpoints();

await app.RunAsync();

static RateLimitPartition<string> CreateAnonymousFixedWindowLimiter(
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

static bool TryGetAnonymousPublicRouteRateLimitPolicy(
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
