using static FastEndpoints.Ep;

namespace SummerBornInfo.Web.Features.EstablishmentStatusCommands.Create;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("establishment-status");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var status = new EstablishmentStatus
        {
            Code = req.Code,
            Name = req.Name
        };

        context.Add(status);
        await context.SaveChangesAsync(c);

        var resp = new Response
        {
            Code = status.Code,
            Name = status.Name,
            Id = status.Id
        };

        await SendAsync(response: resp, cancellation: c);
    }
}