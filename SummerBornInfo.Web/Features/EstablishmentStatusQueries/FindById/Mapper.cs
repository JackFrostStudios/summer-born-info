namespace SummerBornInfo.Web.Features.EstablishmentStatusQueries.FindById;

internal sealed class Mapper : ResponseMapper<Response, EstablishmentStatus>
{
    public override Response FromEntity(EstablishmentStatus e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}