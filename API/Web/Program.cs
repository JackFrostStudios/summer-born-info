using Domain.Entities;
using Features.Schools.Import;
using Features.Schools.Queries.GetAllSchools;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure PostgreSQL database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register handlers
builder.Services.AddScoped<ImportSchoolsCommandHandler>();
builder.Services.AddScoped<GetAllSchoolsQueryHandler>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
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