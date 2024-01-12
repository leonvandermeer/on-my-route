namespace InnoSetup.Tasks;

static class Helpers {
    public static string RelativePathTo(string from, string to) {
        from = Path.GetFullPath(from);
        to = Path.GetFullPath(to);
        Uri uriFrom = new(from);
        Uri uriTo = new(to);
        Uri relative = uriFrom.MakeRelativeUri(uriTo);
        string result = Uri.UnescapeDataString(relative.ToString());
        return result.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }
}
