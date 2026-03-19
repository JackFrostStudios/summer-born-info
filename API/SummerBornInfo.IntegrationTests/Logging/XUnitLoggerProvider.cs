namespace SummerBornInfo.TestFramework.Logging;

public sealed class XUnitLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    private readonly LoggerExternalScopeProvider _scopeProvider = new();

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(_testOutputHelper, _scopeProvider, categoryName);
    }

    public void Dispose()
    {
    }
}