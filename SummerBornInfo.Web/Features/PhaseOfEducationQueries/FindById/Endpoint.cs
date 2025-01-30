namespace SummerBornInfo.Web.Features.PhaseOfEducationQueries.FindById;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;
    public override void Configure()
    {
        AllowAnonymous();
        Get("phase-of-education/{id}");
        Description(g => g.ProducesProblem(404));
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellation)
    {
        var phase = await context.PhaseOfEducation.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken: cancellation);
        if (phase == null)
        {
            await SendNotFoundAsync(cancellation);
        }
        else
        {
            await SendAsync(response: Map.FromEntity(phase), cancellation: cancellation);
            return;
        }
    }
}