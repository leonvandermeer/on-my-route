using Microsoft.Extensions.Options;
using System.Diagnostics;
using Updates.Types;

namespace Updates.Updates;

public class UpdateServer(UpdateProvider updateProvider, IOptions<UpdateData> updateData) : IUpdateServer {
    private readonly UpdateData updateData = updateData.Value;

    public async Task<Release?> CheckForUpdateAsync(bool includePrerelease) {
        return await updateProvider.GetAvailableRelease(
            updateData.Owner, updateData.RepositoryName, updateData.CurrentVersion, includePrerelease
        );
    }

    public async Task<Update> DownloadAsync(Release release) {
        string root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            updateData.Company, updateData.Product, "Updates", release.Manifest.Version
        );
        Directory.CreateDirectory(root);

        foreach (Asset a in release.Assets) {
            string dest = Path.Combine(root, a.Name);
            string temp = Path.Combine(root, dest + ".downloading.tmp");
            if (File.Exists(temp) || !File.Exists(dest)) {
                File.Delete(dest);
                await updateProvider.DownloadAsync(a.BrowserDownloadUrl, temp);
                File.Move(temp, dest);
            }
        }

        string installer = Path.Combine(root, release.Manifest.Installer);
        return new Update(release, installer);
    }

    public void StartInstallation(Update update) {
        Process.Start(update.Installer);
    }
}
