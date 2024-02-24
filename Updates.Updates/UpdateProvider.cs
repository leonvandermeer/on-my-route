using NuGet.Versioning;
using Octokit;
using Updates.Types;
using Release = Updates.Types.Release;

namespace Updates.Updates;

public class UpdateProvider(IReleasesClient release, HttpClient client) {
    class ReleaseComparer : IComparer<Release> {
        public int Compare(Release? x, Release? y) =>
            Comparer<SemanticVersion>.Default.Compare(
                x!.Manifest.GetVersionAsSemanticVersion(),
                y!.Manifest.GetVersionAsSemanticVersion()
            );
    }

    static readonly Func<Release, Release> Identity = r => r;
    static readonly ReleaseComparer releaseComparer = new();

    public async Task<Release?> GetAvailableRelease(string owner, string repositoryName, string currentVersion, bool includePrerelease) {
        Release[] releases = await GetAvailableReleases(owner, repositoryName, currentVersion, includePrerelease);
        return releases.OrderByDescending(Identity, releaseComparer).FirstOrDefault();
    }

    public async Task<Release[]> GetAvailableReleases(string owner, string repositoryName, string currentVersion, bool includePrerelease) {
        SemanticVersion version = SemanticVersion.Parse(currentVersion);
        List<Release> releases = [];
        await foreach (Octokit.Release r in GetAllReleases(owner, repositoryName, 10)) {
            if (!r.Draft && (includePrerelease || !r.Prerelease) && r.Assets.All(a => a.State == "uploaded")) {
                Asset[] assets = (
                    from a in r.Assets
                    select new Asset(a.Name, a.BrowserDownloadUrl, a.Size)).ToArray();
                Asset? m = assets.FirstOrDefault(a => a.Name == "manifest.json");
                if (m != null) {
                    string rawManifest = await client.GetStringAsync(m.BrowserDownloadUrl);
                    Manifest? manifest = rawManifest.GetManifest();
                    if (manifest != null) {
                        Release release = new(r.Name, r.Prerelease, r.HtmlUrl, r.PublishedAt!.Value, assets, manifest);
                        if (release.Manifest.GetVersionAsSemanticVersion() == version) {
                            break;
                        }
                        releases.Add(release);
                    }
                }
            }
        }
        return [.. releases];
    }

    public async IAsyncEnumerable<Octokit.Release> GetAllReleases(string owner, string repositoryName, int pageSize) {
        ApiOptions options = new() { StartPage = 1, PageSize = pageSize, PageCount = 1 };
        IReadOnlyList<Octokit.Release> releases;
        do {
            releases = await release.GetAll(owner, repositoryName, options);
            foreach (Octokit.Release item in releases) {
                yield return item;
            }
            options.StartPage++;
        } while (releases.Count == options.PageSize);
    }

    public async Task DownloadAsync(string url, string path) {
        await using Stream s = await client.GetStreamAsync(url);
        await using FileStream t = File.Create(path);
        await s.CopyToAsync(t);
    }
}
