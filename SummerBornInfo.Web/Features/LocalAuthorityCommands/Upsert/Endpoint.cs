namespace SummerBornInfo.Web.Features.LocalAuthorityCommands.Upsert;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("local-authority");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var localAuthority = await context.LocalAuthority.FirstOrDefaultAsync(la => la.Code == req.Code, c);
        if (localAuthority != null)
        {
            localAuthority.Name = req.Name;
        }
        else
        {
            localAuthority = Map.ToEntity(req);
            context.Add(localAuthority);
        }

        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(localAuthority);

        await SendAsync(response: resp, cancellation: c);
    }
}