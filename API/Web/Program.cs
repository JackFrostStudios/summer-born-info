using Domain.Entities;
using Features.Schools.Commands.CreateSchool;
using Features.Schools.Queries.GetAllSchools;
using Features.Schools.Queries.GetSchoolById;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure PostgreSQL database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateSchoolCommand).Assembly));

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

schools.MapPost("/", async (CreateSchoolCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Created($"/api/schools/{result.Id}", result);
});

schools.MapGet("/", async (IMediator mediator) =>
{
    var schools = await mediator.Send(new GetAllSchoolsQuery());
    return Results.Ok(schools);
});

schools.MapGet("/{id:int}", async (int id, IMediator mediator) =>
{
    var school = await mediator.Send(new GetSchoolByIdQuery(id));
    return school is not null ? Results.Ok(school) : Results.NotFound();
});

app.Run();