using Extensions.Logging.SingleRollingFile;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OnMyRoute;

class CrashHandler(IFlushLoggers? flushLoggers, IOptions<CrashHandlerOptions> options, ILogger<CrashHandler> logger) {
    private readonly DiagnosticsClient diagnosticsClient = new(Environment.ProcessId);
    private readonly string dumpPath = GetDumpPath(options);

    public CrashHandler(IOptions<CrashHandlerOptions> options, ILogger<CrashHandler> logger) : this(null, options, logger) { }

    public void Initialize(string product, string currentVersion) {
        logger.StartApplication(product, currentVersion);
        logger.RuntimeEnvironment(Environment.OSVersion, RuntimeInformation.RuntimeIdentifier);

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += (sender, e) => throw e.Exception;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        Log(e);
        CreateCoreDump();
    }

    private void Log(UnhandledExceptionEventArgs e) {
        if (e.ExceptionObject is RuntimeWrappedException rwe) {
            logger.UnhandledException(rwe.WrappedException);
        } else if (e.ExceptionObject is Exception ex) {
            logger.UnhandledException(ex);
        } else {
            logger.UnhandledException(e.ExceptionObject);
        }
        flushLoggers?.Flush();
    }

    private void CreateCoreDump() {
        diagnosticsClient.WriteDump(
            DumpType.Full,
            dumpPath,
            false
        );
    }

    private static string GetDumpPath(IOptions<CrashHandlerOptions> options) {
        string dumpPath = Environment.ExpandEnvironmentVariables(options.Value.DumpPath);
        if (dumpPath.Contains(' ')) {
            dumpPath = $"\"{dumpPath}\"";
        }
        return dumpPath;
    }
}
