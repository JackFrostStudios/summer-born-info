namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.Exceptions;

public sealed class SchoolBulkImportRowParseException(string message, Exception innerException)
    : SchoolBulkImportException(message, innerException);
