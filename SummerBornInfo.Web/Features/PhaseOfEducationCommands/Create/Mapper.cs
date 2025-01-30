namespace SummerBornInfo.Web.Features.PhaseOfEducationCommands.Create;

internal sealed class Mapper : Mapper<Request, Response, PhaseOfEducation>
{
    public override PhaseOfEducation ToEntity(Request r) => new()
    {
        Code = r.Code,
        Name = r.Name
    };

    public override Response FromEntity(PhaseOfEducation e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}