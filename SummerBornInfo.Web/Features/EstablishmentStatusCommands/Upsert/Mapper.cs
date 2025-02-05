namespace SummerBornInfo.Web.Features.EstablishmentStatusCommands.Upsert;

internal sealed class Mapper : Mapper<Request, Response, EstablishmentStatus>
{
    public override EstablishmentStatus ToEntity(Request r) => new()
    {
        Code = r.Code,
        Name = r.Name
    };

    public override Response FromEntity(EstablishmentStatus e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}