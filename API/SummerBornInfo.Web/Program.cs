var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi(options => options.AddAdminSecurityMetadata());
builder.Services.Configure<SchoolBulkImportWorkerOptions>(builder.Configuration.GetSection(SchoolBulkImportWorkerOptions.SectionName));
builder.Services.Configure<DevelopmentAdminBootstrapOptions>(builder.Configuration);

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
builder.Services.AddScoped<ModerateCsaApplicationReviewCommandHandler>();
builder.Services.AddScoped<ImportSchoolsCommandHandler>();
builder.Services.AddScoped<IProcessImportFileCommandHandler, ProcessImportFileCommandHandler>();
builder.Services.AddScoped<GetAllSchoolsQueryHandler>();
builder.Services.AddScoped<GetNearbySchoolsQueryHandler>();
builder.Services.AddScoped<SearchSchoolsQueryHandler>();
builder.Services.AddScoped<GetSchoolByUrnQueryHandler>();
builder.Services.AddScoped<GetSchoolBulkImportStatusQueryHandler>();
builder.Services.AddScoped<ISchoolsImporter, SchoolsImporter<ApplicationDbContext>>();
builder.Services.AddScoped<ILargeObjectWriter, LargeObjectWriter>();
builder.Services.AddScoped<ILargeObjectReader, LargeObjectReader>();
builder.Services.AddScoped<IEventEmitter, EventEmitter>();
builder.Services.AddScoped<IEventReader, EventReader>();
builder.Services.AddScoped<IEventAcknowledger, EventAcknowledger>();
builder.Services.AddHostedService<ProcessSchoolBulkImportBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await PostgreSqlDatabaseBootstrapper.EnsureApplicationDatabaseAsync(dbContext, app.Lifetime.ApplicationStopping);
    var developmentAdminBootstrapper = scope.ServiceProvider.GetRequiredService<IDevelopmentAdminBootstrapper>();
    await developmentAdminBootstrapper.UpsertAsync(app.Lifetime.ApplicationStopping);
    NpgmqClient npgmq = new(connectionString: dbContext.Database.GetConnectionString() ?? throw new InvalidOperationException("Db Connection string is null"));
    await npgmq.InitAsync(app.Lifetime.ApplicationStopping);
    await npgmq.CreateQueueAsync(EventQueue.SchoolBulkImport.Name, app.Lifetime.ApplicationStopping);
    _ = app.MapOpenApi();
    _ = app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.RegisterAdminAuthEndpoints();
app.RegisterAdminCsaApplicationReviewModerationEndpoints();
app.RegisterAdminSchoolImportEndpoints();
app.RegisterSchoolEndpoints();

await app.RunAsync();
