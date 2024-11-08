namespace SummerBornInfo.Web.Features.EstablishmentTypeCommands.Create;

internal sealed class Mapper : Mapper<Request, Response, EstablishmentType>
{
    public override EstablishmentType ToEntity(Request r) => new()
    {
        Code = r.Code,
        Name = r.Name
    };

    public override Response FromEntity(EstablishmentType e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}