namespace SummerBornInfo.Web.Features.SchoolCommands.Create;

internal sealed class Request
{
    public required int URN { get; init; }
    public required int UKPRN { get; init; }
    public required int EstablishmentNumber { get; init; }
    public required string Name { get; init; }
    public required AddressRequest Address { get; init; }
    public required DateOnly OpenDate { get; init; }
    public DateOnly CloseDate { get; init; }
    public required Guid PhaseOfEducationId { get; init; }
    public required Guid LocalAuthorityId { get; init; }
    public required Guid EstablishmentTypeId { get; init; }
    public required Guid EstablishmentGroupId { get; init; }
    public required Guid EstablishmentStatusId { get; init; }

    internal sealed class AddressRequest
    {
        public string? Street { get; init; }
        public string? Locality { get; init; }
        public string? AddressThree { get; init; }
        public string? Town { get; init; }
        public string? County { get; init; }
        public string? PostCode { get; init; }
    }
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.URN)
                .NotEmpty()
                .WithMessage("URN is required");

            RuleFor(x => x.UKPRN)
                .NotEmpty()
                .WithMessage("UKPRN is required");

            RuleFor(x => x.EstablishmentNumber)
                .NotEmpty()
                .WithMessage("Establishment Number is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required");

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("Address is required");

            RuleFor(x => x.OpenDate)
                .NotEmpty()
                .WithMessage("Open Date is required");


            RuleFor(x => x.PhaseOfEducationId)
                .NotEmpty()
                .WithMessage("Phase of Education Id is required");

            RuleFor(x => x.LocalAuthorityId)
                .NotEmpty()
                .WithMessage("Local Authority Id is required");

            RuleFor(x => x.EstablishmentTypeId)
                .NotEmpty()
                .WithMessage("Establishment Type Id is required");

            RuleFor(x => x.EstablishmentGroupId)
                .NotEmpty()
                .WithMessage("Establishment Group Id is required");

            RuleFor(x => x.EstablishmentStatusId)
                .NotEmpty()
                .WithMessage("Establishment Status Id is required");
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
    public required Guid PhaseOfEducationId { get; init; }
    public required Guid LocalAuthorityId { get; init; }
    public required Guid EstablishmentTypeId { get; init; }
    public required Guid EstablishmentGroupId { get; init; }
    public required Guid EstablishmentStatusId { get; init; }

    internal sealed class AddressResponse
    {
        public string? Street { get; init; }
        public string? Locality { get; init; }
        public string? AddressThree { get; init; }
        public string? Town { get; init; }
        public string? County { get; init; }
        public string? PostCode { get; init; }
    }
}