using NuGet.Versioning;
using System.Text.Json;
using Updates.Types;

namespace Updates.Updates;

public static class ManifestExtensions {

    public static Manifest? GetManifest(this string rawManifest) {
        Version manifestVersion = JsonSerializer.Deserialize<BareManifest>(rawManifest)!.ManifestVersion;
        if (manifestVersion > Manifest.CurrentManifestVersion) { return null; }
        return JsonSerializer.Deserialize<Manifest>(rawManifest)!;
    }

    record BareManifest(Version ManifestVersion);

    public static SemanticVersion GetVersionAsSemanticVersion(this Manifest manifest) =>
        SemanticVersion.Parse(manifest.Version);
}
