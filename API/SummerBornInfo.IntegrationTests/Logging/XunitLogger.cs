namespace SummerBornInfo.TestFramework.Logging;

public class XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string? categoryName) : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    private readonly string? _categoryName = categoryName;
    private readonly LoggerExternalScopeProvider _scopeProvider = scopeProvider;

    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper)
    {
        return new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), "");
    }

    public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper) where T : notnull
    {
        return new GenericXUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable? BeginScope<TState>(TState? state) where TState : notnull
    {
        return _scopeProvider.Push(state);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        StringBuilder sb = new();
        _ = sb.Append(GetLogLevelString(logLevel))
          .Append(" [").Append(_categoryName).Append("] ")
          .Append(formatter(state, exception));

        if (exception != null)
        {
            _ = sb.Append('\n').Append(exception);
        }

        // Append scopes
        _scopeProvider.ForEachScope((scope, state) =>
        {
            _ = state.Append("\n => ");
            _ = state.Append(scope);
        }, sb);

        _testOutputHelper.WriteLine(sb.ToString());
    }
    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            LogLevel.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        };
    }

}
