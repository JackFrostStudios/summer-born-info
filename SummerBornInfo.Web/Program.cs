var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<SchoolContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("summerborn_info")));
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();
if (app.Environment.IsProduction())
{
    app.UseDefaultExceptionHandler();
}
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
});
app.UseSwaggerGen();

app.Run();

// Make the implicit Program class so test projects can access it
public partial class Program { }