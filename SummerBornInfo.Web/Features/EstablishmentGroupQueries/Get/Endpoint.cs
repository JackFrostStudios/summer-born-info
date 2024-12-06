namespace SummerBornInfo.Web.Features.EstablishmentGroupQueries.Get;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;
    public override void Configure()
    {
        AllowAnonymous();
        Get("establishment-group/{id}");
        Description(g => g.ProducesProblem(404));
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellation)
    {
        var group = await context.EstablishmentGroup.AsNoTracking().FirstOrDefaultAsync(eg => eg.Id == request.Id, cancellationToken: cancellation);
        if (group == null)
        {
            await SendNotFoundAsync(cancellation);
        }
        else
        {
            await SendAsync(response: Map.FromEntity(group), cancellation: cancellation);
            return;
        }
    }
}