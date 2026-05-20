namespace SummerBornInfo.Domain.Entities;

public sealed class SchoolBulkImportFailure
{
    private SchoolBulkImportFailure()
    {
    }

    public SchoolBulkImportFailure(int lineNumber, string errorMessage)
    {
        LineNumber = lineNumber;
        ErrorMessage = errorMessage;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid SchoolBulkImportRequestId { get; private set; }
    public int LineNumber { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    internal void UpdateError(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
