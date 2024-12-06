namespace SummerBornInfo.Web.Features.LocalAuthorityQueries.FindById;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;
    public override void Configure()
    {
        AllowAnonymous();
        Get("local-authority/{id}");
        Description(g => g.ProducesProblem(404));
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellation)
    {
        var authority = await context.LocalAuthority.AsNoTracking().FirstOrDefaultAsync(la => la.Id == request.Id, cancellationToken: cancellation);
        if (authority == null)
        {
            await SendNotFoundAsync(cancellation);
        }
        else
        {
            await SendAsync(response: Map.FromEntity(authority), cancellation: cancellation);
            return;
        }
    }
}