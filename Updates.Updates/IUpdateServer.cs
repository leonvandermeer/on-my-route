using Updates.Types;

namespace Updates.Updates;

public interface IUpdateServer {
    Task<Release?> CheckForUpdateAsync(bool includePrerelease);
    Task<Update> DownloadAsync(Release release);
    void StartInstallation(Update update);
}
