namespace SummerBornInfo.Web.Features.LocalAuthorityCommands.Create;

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
        var localAuthority = Map.ToEntity(req);

        context.Add(localAuthority);
        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(localAuthority);

        await SendAsync(response: resp, cancellation: c);
    }
}