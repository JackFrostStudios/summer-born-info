var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ImportSchoolsCommandHandler>();
builder.Services.AddScoped<GetAllSchoolsQueryHandler>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

// Schools API endpoints
var schools = app.MapGroup("/api/schools");

schools.MapPost("/import", async (IFormFile csvFile, ImportSchoolsCommandHandler handler) =>
{
    if (csvFile == null || csvFile.Length == 0)
    {
        return Results.BadRequest("CSV file is required");
    }

    var stream = csvFile.OpenReadStream();
    var command = new ImportSchoolsCommand(stream, csvFile.FileName);
    var result = await handler.ExecuteAsync(command, CancellationToken.None);
    return Results.Ok(result);
});

schools.MapGet("/", async (GetAllSchoolsQueryHandler handler, Guid? cursor, int? pageSize) =>
{
    var query = new GetAllSchoolsQuery(cursor, pageSize ?? 100);
    var (schools, nextCursor) = await handler.ExecuteAsync(query, CancellationToken.None);
    return Results.Ok(new { schools, nextCursor });
});

app.Run();