namespace SummerBornInfo.Web.Features.EstablishmentTypeCommands.Create;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("establishment-type");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var type = Map.ToEntity(req);

        context.Add(type);
        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(type);

        await SendAsync(response: resp, cancellation: c);
    }
}