namespace SummerBornInfo.Web.Features.SchoolQueries.FindById;

internal sealed class Request
{
    public Guid Id { get; set; }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

internal sealed class Response
{
    public required Guid Id { get; init; }
    public required int URN { get; init; }
    public required int UKPRN { get; init; }
    public required int EstablishmentNumber { get; init; }
    public required string Name { get; init; }
    public required AddressResponse Address { get; init; }
    public required DateOnly OpenDate { get; init; }
    public DateOnly CloseDate { get; init; }
    public required CodeNameEntityResponse PhaseOfEducation { get; init; }
    public required CodeNameEntityResponse LocalAuthority { get; init; }
    public required CodeNameEntityResponse EstablishmentType { get; init; }
    public required CodeNameEntityResponse EstablishmentGroup { get; init; }
    public required CodeNameEntityResponse EstablishmentStatus { get; init; }

    internal sealed class AddressResponse
    {
        public string? Street { get; init; }
        public string? Locality { get; init; }
        public string? AddressThree { get; init; }
        public string? Town { get; init; }
        public string? County { get; init; }
        public string? PostCode { get; init; }
    }

    internal sealed class CodeNameEntityResponse
    {
        public required Guid Id { get; init; }
        public required string Code { get; init; }
        public required string Name { get; init; }
    }
}
