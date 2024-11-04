using System.Threading;

namespace Features.EstablishmentGroup.Create;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("establishment-groups");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var group = new SummerBornInfo.Domain.EstablishmentGroup
        {
            Code = req.Code,
            Name = req.Name
        };

        context.Add(group);
        await context.SaveChangesAsync(c);
        
        var resp = new Response
        {
            Code = group.Code,
            Name = group.Name,
            Id = group.Id
        };

        await SendAsync(response: resp, cancellation: c);
    }
}