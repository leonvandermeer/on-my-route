using Extensions.Logging.SingleRollingFile;
using Octokit;
using OnMyRoute;
using OnMyRoute.Extensions.DependencyInjection;
using System.Reflection;
using Updates.Updates;

using Mutex appMutex = new(false, @"Global\On My Route AppMutex");

Type type = typeof(Program);
string product = type.Assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product;
string currentVersion = type.Assembly.GetCustomAttribute<CurrentVersionAttribute>()!.Version;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddWpf<App>();
builder.Services
    .AddCrashHandler(product, currentVersion)
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
        o.Owner = "leonvandermeer";
        o.RepositoryName = "on-my-route";
        o.Company = type.Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()!.Company;
        o.Product = product;
        o.CurrentVersion = currentVersion;
        o.InstallUpdateWithin = TimeSpan.FromDays(1);
    });
builder.Logging.AddRollingFile();
IHost host = builder.Build();
await host.RunAsync();
