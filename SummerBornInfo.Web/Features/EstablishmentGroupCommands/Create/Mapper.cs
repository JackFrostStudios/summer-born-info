namespace SummerBornInfo.Web.Features.EstablishmentGroupCommands.Create;

internal sealed class Mapper : Mapper<Request, Response, EstablishmentGroup>
{
    public override EstablishmentGroup ToEntity(Request r) => new()
    {
        Code = r.Code,
        Name = r.Name
    };

    public override Response FromEntity(EstablishmentGroup e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}