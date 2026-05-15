using System.Diagnostics;

namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile;

public static class SchoolBulkImportTelemetry
{
    public const string ActivityName = "ProcessSchoolBulkImport";
    public const string ActivitySourceName = "SummerBornInfo.Features";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
