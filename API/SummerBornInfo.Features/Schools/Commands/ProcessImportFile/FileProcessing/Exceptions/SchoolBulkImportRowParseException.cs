namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.Exceptions;

public sealed class SchoolBulkImportRowParseException(Exception innerException)
    : SchoolBulkImportException("Unable to parse CSV row.", innerException);
