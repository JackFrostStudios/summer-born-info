namespace SummerBornInfo.Web.Features.EstablishmentTypeQueries.FindById;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;
    public override void Configure()
    {
        AllowAnonymous();
        Get("establishment-type/{id}");
        Description(g => g.ProducesProblem(404));
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellation)
    {
        var type = await context.EstablishmentType.AsNoTracking().FirstOrDefaultAsync(et => et.Id == request.Id, cancellationToken: cancellation);
        if (type == null)
        {
            await SendNotFoundAsync(cancellation);
        }
        else
        {
            await SendAsync(response: Map.FromEntity(type), cancellation: cancellation);
            return;
        }
    }
}