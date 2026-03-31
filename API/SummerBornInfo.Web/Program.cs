using SummerBornInfo.Infrastructure.Persistence.LargeObjects;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("SummerbornInfo");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<ImportSchoolsCommandHandler>();
builder.Services.AddScoped<GetAllSchoolsQueryHandler>();
builder.Services.AddScoped<ILargeObjectWriter,  LargeObjectWriter>();
builder.Services.AddScoped<IEventEmitter,  EventEmitter>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    var scope = app.Services.CreateScope();
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