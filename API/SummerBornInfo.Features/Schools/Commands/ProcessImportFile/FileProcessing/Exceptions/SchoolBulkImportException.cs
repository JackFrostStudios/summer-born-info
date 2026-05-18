namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.Exceptions;

public abstract class SchoolBulkImportException(string message, Exception? innerException = null)
    : Exception(message, innerException);
