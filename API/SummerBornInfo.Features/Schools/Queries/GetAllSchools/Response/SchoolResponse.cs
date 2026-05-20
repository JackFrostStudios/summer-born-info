namespace SummerBornInfo.Features.Schools.Queries.GetAllSchools.Response;

public sealed record SchoolResponse
{
    public required Guid Id { get; init; }
    public required int URN { get; init; }
    public required int? UKPRN { get; init; }
    public required int EstablishmentNumber { get; init; }
    public required string Name { get; init; }
    public required SchoolAddressResponse Address { get; init; }
    public required DateOnly? OpenDate { get; init; }
    public required DateOnly? CloseDate { get; init; }
    public required PhaseOfEducationResponse PhaseOfEducation { get; init; }
    public required LocalAuthorityResponse LocalAuthority { get; init; }
    public required EstablishmentTypeResponse EstablishmentType { get; init; }
    public required EstablishmentGroupResponse EstablishmentGroup { get; init; }
    public required EstablishmentStatusResponse EstablishmentStatus { get; init; }
    public static SchoolResponse FromEntity(School school)
    {
        return new()
        {
            Id = school.Id,
            URN = school.URN,
            UKPRN = school.UKPRN,
            EstablishmentNumber = school.EstablishmentNumber,
            Name = school.Name,
            Address = SchoolAddressResponse.FromEntity(school.Address),
            OpenDate = school.OpenDate,
            CloseDate = school.CloseDate,
            PhaseOfEducation = PhaseOfEducationResponse.FromEntity(school.PhaseOfEducation),
            LocalAuthority = LocalAuthorityResponse.FromEntity(school.LocalAuthority),
            EstablishmentType = EstablishmentTypeResponse.FromEntity(school.EstablishmentType),
            EstablishmentGroup = EstablishmentGroupResponse.FromEntity(school.EstablishmentGroup),
            EstablishmentStatus = EstablishmentStatusResponse.FromEntity(school.EstablishmentStatus),
        };
    }
};
