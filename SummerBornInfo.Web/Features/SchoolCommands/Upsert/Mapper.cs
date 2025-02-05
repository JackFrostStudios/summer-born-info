namespace SummerBornInfo.Web.Features.SchoolCommands.Upsert;

internal sealed class Mapper : ResponseMapper<Response, School>
{
    public override Response FromEntity(School e) => new()
    {
        Id = e.Id,
        URN = e.URN,
        UKPRN = e.UKPRN,
        EstablishmentNumber = e.EstablishmentNumber,
        Name = e.Name,
        Address = new()
        {
            Street = e.Address.Street,
            Locality = e.Address.Locality,
            AddressThree = e.Address.AddressThree,
            Town = e.Address.Town,
            County = e.Address.County,
            PostCode = e.Address.PostCode,
        },
        OpenDate = e.OpenDate,
        CloseDate = e.CloseDate,
        PhaseOfEducationId = e.PhaseOfEducation.Id,
        LocalAuthorityId = e.LocalAuthority.Id,
        EstablishmentTypeId = e.EstablishmentType.Id,
        EstablishmentGroupId = e.EstablishmentGroup.Id,
        EstablishmentStatusId = e.EstablishmentStatus.Id,
    };
}