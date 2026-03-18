namespace SummerBornInfo.TestFramework.Logging;

public sealed class XUnitLogger<T>(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider)
    : XUnitLogger(testOutputHelper, scopeProvider, typeof(T).FullName), ILogger<T> where T : notnull;