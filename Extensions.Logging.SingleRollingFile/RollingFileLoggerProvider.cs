using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Extensions.Logging.SingleRollingFile;

[ProviderAlias("RollingFile")]
internal class RollingFileLoggerProvider(RollingFileLoggerProcessor processor) : ILoggerProvider, ISupportExternalScope {
    private readonly ConcurrentDictionary<string, RollingFileLogger> loggers = new();
    private IExternalScopeProvider? scopeProvider;

    public ILogger CreateLogger(string categoryName) {
        return loggers.TryGetValue(categoryName, out RollingFileLogger? logger) ?
            logger :
            loggers.GetOrAdd(categoryName, new RollingFileLogger(categoryName, processor, scopeProvider));
    }

    public void Dispose() { }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) {
        this.scopeProvider = scopeProvider;
        foreach (KeyValuePair<string, RollingFileLogger> logger in loggers) {
            logger.Value.ScopeProvider = scopeProvider;
        }
    }
}
