namespace SummerBornInfo.Web.Features.EstablishmentGroupCommands.Upsert;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("establishment-group");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var group = await context.EstablishmentGroup.FirstOrDefaultAsync(eg => eg.Code == req.Code, c);
        if (group != null) {
            group.Name = req.Name;
        } else
        {
            group = Map.ToEntity(req);
            context.Add(group);
        }
        
        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(group);

        await SendAsync(response: resp, cancellation: c);
    }
}