namespace SummerBornInfo.Domain.Entities;

public sealed class SchoolBulkImportRequest
{
    private readonly List<SchoolBulkImportFailure> _failures = [];

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required uint ContentId { get; init; }
    public int LinesProcessed { get; private set; }
    public SchoolBulkImportStatus Status { get; private set; } = SchoolBulkImportStatus.Pending;
    public IReadOnlyList<SchoolBulkImportFailure> Failures => _failures;

    public bool ProcessingStarted()
    {
        if (Status is SchoolBulkImportStatus.Completed or SchoolBulkImportStatus.CompletedWithFailures or SchoolBulkImportStatus.Processing)
        {
            return false;
        }

        Status = SchoolBulkImportStatus.Processing;
        return true;
    }

    public void UpdateProgress(int lineNumber, string? errorMessage)
    {
        LinesProcessed++;

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return;
        }

        var existingFailure = _failures.SingleOrDefault(x => x.LineNumber == lineNumber);
        if (existingFailure is null)
        {
            _failures.Add(new SchoolBulkImportFailure(lineNumber, errorMessage));
            return;
        }

        existingFailure.UpdateError(errorMessage);
    }

    public void ProcessingComplete()
    {
        Status = _failures.Count == 0
            ? SchoolBulkImportStatus.Completed
            : SchoolBulkImportStatus.CompletedWithFailures;
    }

    public void ProcessingFailed()
    {
        Status = SchoolBulkImportStatus.Failed;
    }
}
