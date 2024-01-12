namespace Updates.Types;

public record Release(string Name, bool Prerelease, string HtmlUrl, DateTimeOffset PublishedAt, Asset[] Assets, Manifest Manifest);
