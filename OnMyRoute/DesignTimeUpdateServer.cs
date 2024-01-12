using System.ComponentModel;
using Updates.Types;
using Updates.Updates;

namespace OnMyRoute;

class DesignTimeUpdateServer : IUpdateServer {
    public DesignTimeUpdateServer() {
        if (!DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
            throw new InvalidOperationException("This is only to be used in a designer.");
        }
    }

    public async Task<Release?> CheckForUpdateAsync(bool includePrerelease) {
        await Task.Delay(1000);
        return new("Two", includePrerelease, null!, DateTimeOffset.UtcNow.AddHours(-24), null!, new Manifest("1.0.44", "Setup.1.0.44.exe"));
    }

    public async Task<Update> DownloadAsync(Release release) {
        await Task.Delay(1000);
        return new Update(release, release.Manifest.Installer);
    }

    public void StartInstallation(Update update) => throw new NotImplementedException();
}
