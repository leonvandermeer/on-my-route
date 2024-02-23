namespace OnMyRoute;

static class CrashHandlerExtensions {
    public static IServiceCollection AddCrashHandler(this IServiceCollection services, string product, string currentVersion) =>
        services
            .AddSingleton<CrashHandler>()
            .AddOptions<CrashHandlerOptions>().BindConfiguration(nameof(CrashHandler)).Services
            .AddOptionsWithValidateOnStart<NoopOptions>()
                .Configure<CrashHandler>((o, ch) => ch.Initialize(product, currentVersion)).Services;

    class NoopOptions { }
}
