namespace SummerBornInfo.Web.Features.LocalAuthorityQueries.FindById;

internal sealed class Mapper : ResponseMapper<Response, LocalAuthority>
{
    public override Response FromEntity(LocalAuthority e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}