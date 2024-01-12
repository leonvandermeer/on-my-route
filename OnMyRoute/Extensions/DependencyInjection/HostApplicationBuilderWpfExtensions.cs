using OnMyRoute.Extensions.Hosting;

namespace OnMyRoute.Extensions.DependencyInjection;

static class HostApplicationBuilderWpfExtensions {
    public static HostApplicationBuilder AddWpf<TApplication>(this HostApplicationBuilder builder)
        where TApplication : Application {
        builder.Services.AddHostedService<WpfWorker>();
        _ = builder.Services
            .AddSingleton<IFactory<Application>, Factory<Application, TApplication>>()
            .AddSingleton<Func<TApplication>>(s => () => s.GetRequiredService<TApplication>())
            .AddSingleton<TApplication>();
        return builder;
    }
}
