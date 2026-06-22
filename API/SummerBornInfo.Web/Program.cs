var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

await ConfigureDevelopmentServicesAsync(app);
ConfigureMiddleware(app);
ConfigureEndpoints(app);

await app.RunAsync();

static void ConfigureServices(WebApplicationBuilder builder)
{
    _ = builder.AddServiceDefaults();
    ConfigureOpenApi(builder.Services);
    ConfigureApplicationOptions(builder.Services, builder.Configuration);
    _ = builder.Services.AddAnonymousPublicEndpointAbuseControls(builder.Configuration, builder.Environment);
    ConfigurePersistence(builder);
    ConfigureIdentity(builder.Services);
    ConfigureAuthorization(builder.Services);
    RegisterApplicationServices(builder.Services);
}

static void ConfigureOpenApi(IServiceCollection services)
{
    _ = services.AddOpenApi(options => options.AddAdminSecurityMetadata());
}

static void ConfigureApplicationOptions(IServiceCollection services, IConfiguration configuration)
{
    _ = services.Configure<SchoolBulkImportWorkerOptions>(configuration.GetSection(SchoolBulkImportWorkerOptions.SectionName));
    _ = services.Configure<DevelopmentAdminBootstrapOptions>(configuration);
}

static void ConfigurePersistence(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("SummerbornInfo");
    _ = builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));
}

static void ConfigureIdentity(IServiceCollection services)
{
    _ = services
        .AddIdentity<ApplicationUser, ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    _ = services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = ApiCookieAuthenticationEvents.RedirectToUnauthorizedAsync;
        options.Events.OnRedirectToAccessDenied = ApiCookieAuthenticationEvents.RedirectToForbiddenAsync;
    });
}

static void ConfigureAuthorization(IServiceCollection services)
{
    _ = services
        .AddAuthorizationBuilder()
        .AddPolicy(
            AdminAuthorizationPolicyNames.Admin,
            policy => policy.RequireRole(ApplicationRoleNames.Admin));
}

static void RegisterApplicationServices(IServiceCollection services)
{
    _ = services
        .AddScoped<IDevelopmentAdminBootstrapper, DevelopmentAdminBootstrapper>()
        .AddScoped<GetAdminCsaApplicationReviewQueueQueryHandler>()
        .AddScoped<ModerateCsaApplicationReviewCommandHandler>()
        .AddScoped<SubmitPublicCsaApplicationReviewCommandHandler>()
        .AddScoped<SubmitPublicCsaApplicationReviewReportCommandHandler>()
        .AddScoped<ImportSchoolsCommandHandler>()
        .AddScoped<IProcessImportFileCommandHandler, ProcessImportFileCommandHandler>()
        .AddScoped<GetAllSchoolsQueryHandler>()
        .AddScoped<GetPublicCsaApplicationReviewsQueryHandler>()
        .AddScoped<GetNearbySchoolsQueryHandler>()
        .AddScoped<SearchSchoolsQueryHandler>()
        .AddScoped<GetSchoolByUrnQueryHandler>()
        .AddScoped<GetSchoolBulkImportStatusQueryHandler>()
        .AddSingleton<IBritishNationalGridLocationConverter, BritishNationalGridLocationConverter>()
        .AddScoped<ISchoolsImporter, SchoolsImporter<ApplicationDbContext>>()
        .AddScoped<ILargeObjectWriter, LargeObjectWriter>()
        .AddScoped<ILargeObjectReader, LargeObjectReader>()
        .AddScoped<IEventEmitter, EventEmitter>()
        .AddScoped<IEventReader, EventReader>()
        .AddScoped<IEventAcknowledger, EventAcknowledger>()
        .AddHostedService<ProcessSchoolBulkImportBackgroundService>();
}

static async Task ConfigureDevelopmentServicesAsync(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

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

static void ConfigureMiddleware(WebApplication app)
{
    _ = app
        .UseRouting()
        .UseHttpsRedirection()
        .UseRateLimiter()
        .UseAuthentication()
        .UseAuthorization();
}

static void ConfigureEndpoints(WebApplication app)
{
    app.RegisterAdminAuthEndpoints();
    app.RegisterAdminCsaApplicationReviewModerationEndpoints();
    app.RegisterAdminSchoolImportEndpoints();
    app.RegisterSchoolEndpoints();
}
