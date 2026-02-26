using Domain.Entities;
using Features.Schools.Commands.CreateSchool;
using Features.Schools.Queries.GetAllSchools;
using Features.Schools.Queries.GetSchoolById;
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
builder.Services.AddScoped<CreateSchoolCommandHandler>();
builder.Services.AddScoped<GetAllSchoolsQueryHandler>();
builder.Services.AddScoped<GetSchoolByIdQueryHandler>();

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

schools.MapPost("/", async (CreateSchoolCommand command, CreateSchoolCommandHandler handler) =>
{
    var result = await handler.ExecuteAsync(command, CancellationToken.None);
    return Results.Created($"/api/schools/{result.Id}", result);
});

schools.MapGet("/", async (GetAllSchoolsQueryHandler handler) =>
{
    var schools = await handler.ExecuteAsync(new GetAllSchoolsQuery(), CancellationToken.None);
    return Results.Ok(schools);
});

schools.MapGet("/{id:int}", async (int id, GetSchoolByIdQueryHandler handler) =>
{
    var school = await handler.ExecuteAsync(new GetSchoolByIdQuery(id), CancellationToken.None);
    return school is not null ? Results.Ok(school) : Results.NotFound();
});

app.Run();