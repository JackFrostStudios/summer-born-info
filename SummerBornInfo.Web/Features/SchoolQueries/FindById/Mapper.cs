namespace SummerBornInfo.Web.Features.SchoolQueries.FindById;

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
        PhaseOfEducation = new()
        {
            Id = e.PhaseOfEducation.Id,
            Code = e.PhaseOfEducation.Code,
            Name = e.PhaseOfEducation.Name
        },
        LocalAuthority = new()
        {
            Id = e.LocalAuthority.Id,
            Code = e.LocalAuthority.Code,
            Name = e.LocalAuthority.Name
        },
        EstablishmentType = new()
        {
            Id = e.EstablishmentType.Id,
            Code = e.EstablishmentType.Code,
            Name = e.EstablishmentType.Name
        },
        EstablishmentGroup = new()
        {
            Id = e.EstablishmentGroup.Id,
            Code = e.EstablishmentGroup.Code,
            Name = e.EstablishmentGroup.Name
        },
        EstablishmentStatus = new()
        {
            Id = e.EstablishmentStatus.Id,
            Code = e.EstablishmentStatus.Code,
            Name = e.EstablishmentStatus.Name
        },
    };
}