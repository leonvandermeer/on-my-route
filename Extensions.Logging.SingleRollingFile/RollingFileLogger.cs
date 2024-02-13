using Microsoft.Extensions.Logging;
using System.Text;

namespace Extensions.Logging.SingleRollingFile;

internal class RollingFileLogger(string categoryName, RollingFileLoggerProcessor processor) : ILogger {
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        StringBuilder sb = new();
        sb.Append("Timestamp    = "); sb.AppendLine(DateTimeOffset.Now.ToString("O"));
        sb.Append("categoryName = "); sb.AppendLine(categoryName);
        sb.Append("eventId      = "); sb.AppendLine(eventId.ToString());
        sb.Append("state        = "); sb.AppendLine(state?.ToString());
        sb.Append("exception    = "); sb.AppendLine(exception?.ToString());
        string contents = formatter(state, exception);
        sb.Append("contents     = "); sb.AppendLine(contents);
        sb.AppendLine();
        processor.Enqueue(sb.ToString());
    }
}
