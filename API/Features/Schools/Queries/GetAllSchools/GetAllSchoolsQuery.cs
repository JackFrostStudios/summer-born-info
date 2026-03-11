namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools;

public record GetAllSchoolsQuery(
    Guid? Cursor = null,
    int PageSize = 100
);

public record SchoolDto(
    Guid Id,
    int URN,
    int? UKPRN,
    int EstablishmentNumber,
    string Name,
    SchoolAddressDto Address,
    DateOnly? OpenDate,
    DateOnly? CloseDate,
    PhaseOfEducationDto PhaseOfEducation,
    LocalAuthorityDto LocalAuthority,
    EstablishmentTypeDto EstablishmentType,
    EstablishmentGroupDto EstablishmentGroup,
    EstablishmentStatusDto EstablishmentStatus
)
{
    public static SchoolDto FromEntity(School school) =>
        new(
            school.Id,
            school.URN,
            school.UKPRN,
            school.EstablishmentNumber,
            school.Name,
            SchoolAddressDto.FromEntity(school.Address),
            school.OpenDate,
            school.CloseDate,
            PhaseOfEducationDto.FromEntity(school.PhaseOfEducation),
            LocalAuthorityDto.FromEntity(school.LocalAuthority),
            EstablishmentTypeDto.FromEntity(school.EstablishmentType),
            EstablishmentGroupDto.FromEntity(school.EstablishmentGroup),
            EstablishmentStatusDto.FromEntity(school.EstablishmentStatus)
        );
};
public record SchoolAddressDto(
    Guid SchoolId,
    string? Street,
    string? Locality,
    string? AddressThree,
    string Town,
    string? County,
    string PostCode
)
{
    public static SchoolAddressDto FromEntity(SchoolAddress schoolAddress) => new(
        schoolAddress.SchoolId,
        schoolAddress.Street,
        schoolAddress.Locality,
        schoolAddress.AddressThree,
        schoolAddress.Town,
        schoolAddress.County,
        schoolAddress.PostCode
    );
};

public record PhaseOfEducationDto(
    Guid Id,
    string Code,
    string Name
)
{
    public static PhaseOfEducationDto FromEntity(PhaseOfEducation phaseOfEducation) => new(phaseOfEducation.Id, phaseOfEducation.Code, phaseOfEducation.Name);
}

public record LocalAuthorityDto(
    Guid Id,
    string Code,
    string Name
)
{
    public static LocalAuthorityDto FromEntity(LocalAuthority localAuthority) => new(localAuthority.Id, localAuthority.Code, localAuthority.Name);
}

public record EstablishmentTypeDto(
    Guid Id,
    string Code,
    string Name
)
{
    public static EstablishmentTypeDto FromEntity(EstablishmentType establishmentType) => new(establishmentType.Id, establishmentType.Code, establishmentType.Name);
}

public record EstablishmentGroupDto(
    Guid Id,
    string Code,
    string Name
)
{
    public static EstablishmentGroupDto FromEntity(EstablishmentGroup establishmentGroup) => new(establishmentGroup.Id, establishmentGroup.Code, establishmentGroup.Name);
}

public record EstablishmentStatusDto(
    Guid Id,
    string Code,
    string Name
)
{
    public static EstablishmentStatusDto FromEntity(EstablishmentStatus establishmentStatus) => new(establishmentStatus.Id, establishmentStatus.Code, establishmentStatus.Name);
}