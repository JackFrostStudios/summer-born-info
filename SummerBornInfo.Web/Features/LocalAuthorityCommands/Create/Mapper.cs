namespace SummerBornInfo.Web.Features.LocalAuthorityCommands.Create;

internal sealed class Mapper : Mapper<Request, Response, LocalAuthority>
{
    public override LocalAuthority ToEntity(Request r) => new()
    {
        Code = r.Code,
        Name = r.Name
    };

    public override Response FromEntity(LocalAuthority e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}