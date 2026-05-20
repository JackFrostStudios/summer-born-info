namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.Exceptions;

public sealed class SchoolBulkImportProcessingException(Exception innerException)
    : SchoolBulkImportException("The import file could not be processed. Please try again.", innerException);
