using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Extensions.Logging.SingleRollingFile;

[ProviderAlias("RollingFile")]
internal class RollingFileLoggerProvider(RollingFileLoggerProcessor processor) : ILoggerProvider {
    private readonly ConcurrentDictionary<string, RollingFileLogger> loggers = new();

    public ILogger CreateLogger(string categoryName) {
        return loggers.TryGetValue(categoryName, out RollingFileLogger? logger) ?
            logger :
            loggers.GetOrAdd(categoryName, new RollingFileLogger(categoryName, processor));
    }

    public void Dispose() { }
}
