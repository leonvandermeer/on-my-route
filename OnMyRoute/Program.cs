using Octokit;
using OnMyRoute;
using OnMyRoute.Extensions.DependencyInjection;
using System.Reflection;
using Updates.Updates;

using Mutex appMutex = new(false, @"Global\On My Route AppMutex");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddWpf<App>();
builder.Services
    .AddTransient<MainWindow>()
    .AddTransient<MainViewModel>()
    .AddTransient<UpdatesViewModel>()
    .AddTransient<IUpdateServer, UpdateServer>()
    .AddTransient<UpdateProvider>()
    .AddSingleton<GitHubClientFactory>()
    .AddSingleton(s => s.GetRequiredService<GitHubClientFactory>().Create())
    .AddTransient(s => s.GetRequiredService<IGitHubClient>().Repository.Release)
    .AddHttpClient<UpdateProvider>().Services
    .Configure<UpdateData>(o => {
        Type type = typeof(Program);
        o.Owner = "leonvandermeer";
        o.RepositoryName = "on-my-route";
        o.Company = type.Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()!.Company;
        o.Product = type.Assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product;
        o.CurrentVersion = type.Assembly.GetCustomAttribute<CurrentVersionAttribute>()!.Version;
        o.InstallUpdateWithin = TimeSpan.FromDays(1);
    });
IHost host = builder.Build();
await host.RunAsync();
