namespace SummerBornInfo.Web.Features.SchoolQueries.FindById;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;
    public override void Configure()
    {
        AllowAnonymous();
        Get("school/{id}");
        Description(g => g.ProducesProblem(404));
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellation)
    {
        var school = await context.School.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken: cancellation);
        if (school == null)
        {
            await SendNotFoundAsync(cancellation);
        }
        else
        {
            await SendAsync(response: Map.FromEntity(school), cancellation: cancellation);
            return;
        }
    }
}