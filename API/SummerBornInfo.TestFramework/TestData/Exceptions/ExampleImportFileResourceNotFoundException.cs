namespace SummerBornInfo.TestFramework.TestData.Exceptions;

public sealed class ExampleImportFileResourceNotFoundException(string message) : InvalidOperationException(message);
