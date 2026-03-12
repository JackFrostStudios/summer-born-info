namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools;

public record GetAllSchoolsQuery(
    Guid? Cursor = null,
    int PageSize = 100
);

public record SchoolDto
{
    public required Guid Id { get; init; }
    public required int URN { get; init; }
    public required int? UKPRN { get; init; }
    public required int EstablishmentNumber { get; init; }
    public required string Name { get; init; }
    public required SchoolAddressDto Address { get; init; }
    public required DateOnly? OpenDate { get; init; }
    public required DateOnly? CloseDate { get; init; }
    public required PhaseOfEducationDto PhaseOfEducation { get; init; }
    public required LocalAuthorityDto LocalAuthority { get; init; }
    public required EstablishmentTypeDto EstablishmentType { get; init; }
    public required EstablishmentGroupDto EstablishmentGroup { get; init; }
    public required EstablishmentStatusDto EstablishmentStatus { get; init; }
    public static SchoolDto FromEntity(School school) =>
        new()
        {
            Id = school.Id,
            URN = school.URN,
            UKPRN = school.UKPRN,
            EstablishmentNumber = school.EstablishmentNumber,
            Name = school.Name,
            Address = SchoolAddressDto.FromEntity(school.Address),
            OpenDate = school.OpenDate,
            CloseDate = school.CloseDate,
            PhaseOfEducation = PhaseOfEducationDto.FromEntity(school.PhaseOfEducation),
            LocalAuthority = LocalAuthorityDto.FromEntity(school.LocalAuthority),
            EstablishmentType = EstablishmentTypeDto.FromEntity(school.EstablishmentType),
            EstablishmentGroup = EstablishmentGroupDto.FromEntity(school.EstablishmentGroup),
            EstablishmentStatus = EstablishmentStatusDto.FromEntity(school.EstablishmentStatus)
        };
};
public record SchoolAddressDto
{
    public required Guid SchoolId { get; init; }
    public required string? Street { get; init; }
    public required string? Locality { get; init; }
    public required string? AddressThree { get; init; }
    public required string Town { get; init; }
    public required string? County { get; init; }
    public required string PostCode { get; init; }
    public static SchoolAddressDto FromEntity(SchoolAddress schoolAddress) => new()
    {
        SchoolId = schoolAddress.SchoolId,
        Street = schoolAddress.Street,
        Locality = schoolAddress.Locality,
        AddressThree = schoolAddress.AddressThree,
        Town = schoolAddress.Town,
        County = schoolAddress.County,
        PostCode = schoolAddress.PostCode
    };

};

public record PhaseOfEducationDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static PhaseOfEducationDto FromEntity(PhaseOfEducation phaseOfEducation) =>
        new()
        {
            Id = phaseOfEducation.Id,
            Code = phaseOfEducation.Code,
            Name = phaseOfEducation.Name
        };
}

public record LocalAuthorityDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static LocalAuthorityDto FromEntity(LocalAuthority localAuthority) =>
        new()
        {
            Id = localAuthority.Id,
            Code = localAuthority.Code,
            Name = localAuthority.Name
        };
}

public record EstablishmentTypeDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static EstablishmentTypeDto FromEntity(EstablishmentType establishmentType) =>
        new()
        {
            Id = establishmentType.Id,
            Code = establishmentType.Code,
            Name = establishmentType.Name
        };
}

public record EstablishmentGroupDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static EstablishmentGroupDto FromEntity(EstablishmentGroup establishmentGroup) =>
        new()
        {
            Id = establishmentGroup.Id,
            Code = establishmentGroup.Code,
            Name = establishmentGroup.Name
        };
}

public record EstablishmentStatusDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public static EstablishmentStatusDto FromEntity(EstablishmentStatus establishmentStatus) =>
        new()
        {
            Id = establishmentStatus.Id,
            Code = establishmentStatus.Code,
            Name = establishmentStatus.Name
        };
}