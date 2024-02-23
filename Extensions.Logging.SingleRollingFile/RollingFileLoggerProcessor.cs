using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace Extensions.Logging.SingleRollingFile;

class RollingFileLoggerProcessor : IFlushLoggers, IAsyncDisposable {
    private TextWriter? wr;
    private readonly ChannelWriter<object> w;
    private readonly ChannelReader<object> r;
    private readonly Task t;
    private readonly IDisposable? changeToken;

    public RollingFileLoggerProcessor(IOptionsMonitor<RollingFileLoggerOptions> options) {
        UnboundedChannelOptions options2 = new() {
            SingleWriter = true,
            SingleReader = true
        };
        Channel<object> channel = Channel.CreateUnbounded<object>(options2);
        w = channel;
        r = channel;
        t = Task.Run(ProcessQueueAsync);
        ReloadLoggerOptions(options.CurrentValue);
        changeToken = options.OnChange(ReloadLoggerOptions);
    }

    private void ReloadLoggerOptions(RollingFileLoggerOptions options) {
        _ = w.TryWrite(options);
    }

    private async Task ProcessQueueAsync() {
        await ProcessQueueImplAsync();
        await r.Completion;
    }

    private async Task ProcessQueueImplAsync() {
        while (await r.WaitToReadAsync()) {
            while (r.TryRead(out object? command)) {
                switch (command) {
                    case RollingFileLoggerOptions options:
                        Restart(options);
                        break;
                    case string message:
                        await wr!.WriteAsync(message);
                        break;
                    case TaskCompletionSource tcs:
                        await wr!.FlushAsync();
                        tcs.SetResult();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            await wr!.FlushAsync();
        }
    }

    private void Restart(RollingFileLoggerOptions options) {
        TextWriter? local = wr;
        wr = null;
        local?.Dispose();
        string path = Environment.ExpandEnvironmentVariables(options.Path);
        Stream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
        RollingStream rs = new(fs, options.LowLevel, options.HighLevel);
        wr = new StreamWriter(rs);
    }

    public void Enqueue(string message) {
        if (!w.TryWrite(message)) {
            WriteDirectlyAsync(message).GetAwaiter().GetResult();
        }

        async Task WriteDirectlyAsync(string message) {
            await r.Completion;
            await wr!.WriteAsync(message);
            await wr.FlushAsync();
        }
    }

    public void Flush() {
        TaskCompletionSource tcs = new();
        if (w.TryWrite(tcs)) {
            tcs.Task.GetAwaiter().GetResult();
        }
    }

    public async ValueTask DisposeAsync() {
        changeToken?.Dispose();
        _ = w.TryComplete();
        await t;
        await r.Completion;
    }
}
