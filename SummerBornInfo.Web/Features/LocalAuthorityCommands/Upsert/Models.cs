namespace SummerBornInfo.Web.Features.LocalAuthorityCommands.Upsert;

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
                .WithMessage("Local Authority Code is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Local Authority Name is required");
        }
    }
}

internal sealed class Response
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
}