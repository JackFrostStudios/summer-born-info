var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.Configure<SchoolBulkImportWorkerOptions>(builder.Configuration.GetSection(SchoolBulkImportWorkerOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("SummerbornInfo");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<ImportSchoolsCommandHandler>();
builder.Services.AddScoped<ProcessImportFileCommandHandler>();
builder.Services.AddScoped<GetAllSchoolsQueryHandler>();
builder.Services.AddScoped<GetSchoolBulkImportStatusQueryHandler>();
builder.Services.AddScoped<SchoolsImporter<ApplicationDbContext>>();
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
    await dbContext.Database.EnsureCreatedAsync();
    var npgmq = new NpgmqClient(connectionString: dbContext.Database.GetConnectionString() ?? throw new InvalidOperationException("Db Connection string is null"));
    await npgmq.InitAsync();
    await npgmq.CreateQueueAsync(EventQueue.SchoolBulkImport.Name);
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.RegisterSchoolEndpoints();

app.Run();
