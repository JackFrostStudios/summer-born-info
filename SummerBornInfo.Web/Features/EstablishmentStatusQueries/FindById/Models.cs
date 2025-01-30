namespace SummerBornInfo.Web.Features.EstablishmentStatusQueries.FindById;

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
    public required int Code { get; init; }
    public required string Name { get; init; }
}
