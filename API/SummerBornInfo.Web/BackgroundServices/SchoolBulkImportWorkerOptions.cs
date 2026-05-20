namespace SummerBornInfo.Web.BackgroundServices;

public sealed class SchoolBulkImportWorkerOptions
{
    public const string SectionName = "SchoolBulkImportWorker";

    public int EmptyQueueDelaySeconds { get; init; } = 1;
    public int MessageReadTimeoutSeconds { get; init; } = 30;
    public int MaxRetryCount { get; init; } = 3;
}
