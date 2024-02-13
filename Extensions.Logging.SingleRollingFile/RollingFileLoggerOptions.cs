namespace Extensions.Logging.SingleRollingFile;

public class RollingFileLoggerOptions {
    public required string Path { get; set; }

    public required long LowLevel { get; set; } = 500_000L;

    public required long HighLevel { get; set; } = 1_000_000;
}
