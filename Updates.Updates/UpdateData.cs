namespace Updates.Updates;

public class UpdateData {
    public required string Owner { get; set; }

    public required string RepositoryName { get; set; }

    public required string Company { get; set; }

    public required string Product { get; set; }

    public required string CurrentVersion { get; set; }

    public required TimeSpan InstallUpdateWithin { get; set; }
}
