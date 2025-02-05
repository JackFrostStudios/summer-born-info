namespace SummerBornInfo.Web.Features.EstablishmentStatusCommands.Upsert;

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
        var status = await context.EstablishmentStatus.FirstOrDefaultAsync(es => es.Code == req.Code, c);
        if (status != null)
        {
            status.Name = req.Name;
        }
        else
        {
            status = Map.ToEntity(req);
            context.Add(status);
        }

        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(status);

        await SendAsync(response: resp, cancellation: c);
    }
}