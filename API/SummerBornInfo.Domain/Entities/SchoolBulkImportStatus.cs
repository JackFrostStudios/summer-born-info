namespace SummerBornInfo.Domain.Entities;

public enum SchoolBulkImportStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    CompletedWithFailures = 3,
    Failed = 4,
}
