namespace Updates.Updates;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class CurrentVersionAttribute(string version) : Attribute {
    public string Version { get; } = version;
}
