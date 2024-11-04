var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<SchoolContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("summerborn_info")));
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();