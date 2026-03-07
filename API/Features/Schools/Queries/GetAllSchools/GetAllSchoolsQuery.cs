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
);

public record SchoolAddressDto(
    Guid SchoolId,
    string? Street,
    string? Locality,
    string? AddressThree,
    string Town,
    string? County,
    string PostCode
);

public record PhaseOfEducationDto(
    Guid Id,
    string Code,
    string Name
);

public record LocalAuthorityDto(
    Guid Id,
    string Code,
    string Name
);

public record EstablishmentTypeDto(
    Guid Id,
    string Code,
    string Name
);

public record EstablishmentGroupDto(
    Guid Id,
    string Code,
    string Name
);

public record EstablishmentStatusDto(
    Guid Id,
    string Code,
    string Name
);