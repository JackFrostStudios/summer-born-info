namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile;

public sealed record SchoolImportResult
{
    public required int LineNumber { get; init; }
    public required bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
}
