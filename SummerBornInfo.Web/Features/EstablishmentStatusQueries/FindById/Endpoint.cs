namespace SummerBornInfo.Web.Features.EstablishmentStatusQueries.FindById;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;
    public override void Configure()
    {
        AllowAnonymous();
        Get("establishment-status/{id}");
        Description(g => g.ProducesProblem(404));
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellation)
    {
        var status = await context.EstablishmentStatus.AsNoTracking().FirstOrDefaultAsync(es => es.Id == request.Id, cancellationToken: cancellation);
        if (status == null)
        {
            await SendNotFoundAsync(cancellation);
        }
        else
        {
            await SendAsync(response: Map.FromEntity(status), cancellation: cancellation);
            return;
        }
    }
}