namespace OnMyRoute;

static partial class Log {
    [LoggerMessage(0, LogLevel.Information, "Starting `{product}` Version {version}")]
    public static partial void StartApplication(this ILogger logger, string product, string version);

    [LoggerMessage(1, LogLevel.Information, "OS={os}; RID={runtimeIdentifier}")]
    public static partial void RuntimeEnvironment(this ILogger logger, OperatingSystem os, string runtimeIdentifier);

    [LoggerMessage(2, LogLevel.Critical, "UnhandledException")]
    public static partial void UnhandledException(this ILogger logger, Exception ex);

    [LoggerMessage(3, LogLevel.Critical, "UnhandledException: {ex}")]
    public static partial void UnhandledException(this ILogger logger, object ex);
}
