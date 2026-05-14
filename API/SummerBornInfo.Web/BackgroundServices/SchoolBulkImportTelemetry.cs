using System.Diagnostics;

namespace SummerBornInfo.Web.BackgroundServices;

internal static class SchoolBulkImportTelemetry
{
    internal const string ActivityName = "ProcessSchoolBulkImport";
    internal static readonly string ActivitySourceName = typeof(Program).Assembly.GetName().Name ?? "SummerBornInfo.Web";
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
