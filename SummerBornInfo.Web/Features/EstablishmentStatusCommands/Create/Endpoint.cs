namespace SummerBornInfo.Web.Features.EstablishmentStatusCommands.Create;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("establishment-status");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var status = Map.ToEntity(req);

        context.Add(status);
        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(status);

        await SendAsync(response: resp, cancellation: c);
    }
}