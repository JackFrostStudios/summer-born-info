namespace SummerBornInfo.Web.Features.PhaseOfEducationCommands.Upsert;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("phase-of-education");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var phase = await context.PhaseOfEducation.FirstOrDefaultAsync(p => p.Code == req.Code, c);
        if (phase != null)
        {
            phase.Name = req.Name;
        }
        else
        {
            phase = Map.ToEntity(req);
            context.Add(phase);
        }

        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(phase);

        await SendAsync(response: resp, cancellation: c);
    }
}