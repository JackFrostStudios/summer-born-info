namespace SummerBornInfo.Features.Schools.Commands.Import;

public sealed record ImportSchoolsResponse(
    Guid ImportRequestId,
    string Status)
{
    public const string QueuedStatus = "queued";
}
