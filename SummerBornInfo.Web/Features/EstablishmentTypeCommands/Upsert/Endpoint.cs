namespace SummerBornInfo.Web.Features.EstablishmentTypeCommands.Upsert;

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
        var type = await context.EstablishmentType.FirstOrDefaultAsync(et => et.Code == req.Code, c);
        if (type != null)
        {
            type.Name = req.Name;
        }
        else
        {
            type = Map.ToEntity(req);
            context.Add(type);
        }

        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(type);

        await SendAsync(response: resp, cancellation: c);
    }
}