using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Extensions.Logging.SingleRollingFile;

public static class RollingFileLoggerExtensions {
    public static ILoggingBuilder AddRollingFile(this ILoggingBuilder builder, Action<RollingFileLoggerOptions> configure) {
        builder.AddRollingFile();
        builder.Services.Configure(configure);
        return builder;
    }

    public static ILoggingBuilder AddRollingFile(this ILoggingBuilder builder) {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, RollingFileLoggerProvider>());
        builder.Services.AddSingleton<RollingFileLoggerProcessor>();
        builder.Services.AddSingleton<IFlushLoggers>(s => s.GetService<RollingFileLoggerProcessor>()!);
        LoggerProviderOptions.RegisterProviderOptions<RollingFileLoggerOptions, RollingFileLoggerProvider>(builder.Services);
        return builder;
    }
}
