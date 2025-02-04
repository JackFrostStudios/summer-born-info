namespace SummerBornInfo.Web.Features.EstablishmentGroupCommands.Upsert;

internal sealed class Request
{
    public required int Code { get; init; }
    public required string Name { get; init; }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Establishment Group Code is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Establishment Group Name is required");
        }
    }
}

internal sealed class Response
{
    public required Guid Id { get; init; }
    public required int Code { get; init; }
    public required string Name { get; init; }
}
