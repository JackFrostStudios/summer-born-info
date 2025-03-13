namespace SummerBornInfo.Web.Features.PhaseOfEducationCommands.Upsert;

internal sealed class Request
{
    public required string Code { get; init; }
    public required string Name { get; init; }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Phase of Education Code is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Phase of Education Name is required");
        }
    }
}

internal sealed class Response
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
}