namespace OnMyRoute.Extensions.Hosting;

class WpfWorker : IHostedService {
    private readonly IFactory<Application> factory;
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly Thread thread;

    public WpfWorker(IFactory<Application> factory, IHostApplicationLifetime applicationLifetime) {
        this.factory = factory;
        this.applicationLifetime = applicationLifetime;
        thread = new(WpfSTAThread) {
            Name = nameof(WpfSTAThread)
        };
        thread.SetApartmentState(ApartmentState.STA);
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        thread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        thread.Join();
        return Task.CompletedTask;
    }

    private void WpfSTAThread() {
        Application app = factory.Create();
        app.Run();
        applicationLifetime.StopApplication();
    }
}
