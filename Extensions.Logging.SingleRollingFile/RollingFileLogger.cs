using Microsoft.Extensions.Logging;
using System.Text;

namespace Extensions.Logging.SingleRollingFile;

internal class RollingFileLogger(string categoryName, RollingFileLoggerProcessor processor, IExternalScopeProvider? scopeProvider) : ILogger {
    internal IExternalScopeProvider? ScopeProvider { get; set; } = scopeProvider;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        ScopeProvider?.Push(state);

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        StringBuilder sb = new();
        sb.Append("timestamp    = "); sb.AppendLine(DateTimeOffset.Now.ToString("O"));
        sb.Append("categoryName = "); sb.AppendLine(categoryName);
        sb.Append("eventId      = "); sb.AppendLine(eventId.ToString());
        LogScope(sb);
        sb.Append("state        = "); sb.AppendLine(state?.ToString());
        sb.Append("exception    = "); sb.AppendLine(exception?.ToString());
        string contents = formatter(state, exception);
        sb.Append("contents     = "); sb.AppendLine(contents);
        sb.AppendLine();
        processor.Enqueue(sb.ToString());

        void LogScope(StringBuilder sb) {
            bool labelWritten = false;
            bool scopeWritten = false;
            ScopeProvider?.ForEachScope((scope, state) => {
                scopeWritten = true;
                if (!labelWritten) {
                    labelWritten = true;
                    state.Append("scope        = ");
                }
                state.Append(" => ");
                state.Append(scope);
            }, sb);
            if (scopeWritten) {
                sb.AppendLine();
            }
        }
    }
}
