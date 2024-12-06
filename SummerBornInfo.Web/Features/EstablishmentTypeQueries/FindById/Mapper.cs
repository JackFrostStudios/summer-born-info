namespace SummerBornInfo.Web.Features.EstablishmentTypeQueries.FindById;

internal sealed class Mapper : ResponseMapper<Response, EstablishmentType>
{
    public override Response FromEntity(EstablishmentType e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}