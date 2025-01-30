namespace SummerBornInfo.Web.Features.PhaseOfEducationQueries.FindById;

internal sealed class Mapper : ResponseMapper<Response, PhaseOfEducation>
{
    public override Response FromEntity(PhaseOfEducation e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}