using System.Buffers;

namespace Extensions.Logging.SingleRollingFile;

public class RollingStream(Stream inner, long low, long high) : AsyncStream(inner) {
    private long position;

    public override ValueTask DisposeAsync() {
        GC.SuppressFinalize(this);
        return Inner.DisposeAsync();
    }

    public override bool CanWrite => Inner.CanWrite;

    public override bool CanSeek => Inner.CanSeek;

    public override long Length => position;

    public override long Position {
        get => position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {
        if (cancellationToken.IsCancellationRequested) {
            return ValueTask.FromCanceled(cancellationToken);
        }
        return WriteCoreAsync(buffer, cancellationToken);

        async ValueTask WriteCoreAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken) {
            Interlocked.Add(ref position, buffer.Length);
            long newPos = Inner.Position + buffer.Length;
            if (newPos > high) {
                await RollAsync(newPos, cancellationToken);
            }
            await Inner.WriteAsync(buffer, cancellationToken);
        }
    }

    private async ValueTask RollAsync(long newPos, CancellationToken cancellationToken) {
        //                                                       Inner.Position
        // to           low-extra                   from               |          newPos
        // |abcdefghijklmnop|-------------|-----------|abcdefghijklmnop|-------|-----|
        //                               low                                 high
        //  <--- length ---> <-- extra -->             <--- length ---> <-- extra -->
        long extra = newPos - Inner.Position;
        long length = Math.Max(0, low - extra);
        if (length > 0) {
            long from = Inner.Position - length;
            long to = 0L;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
            try {
                long count = length;
                while (count > 0) {
                    Inner.Position = from;
                    int bytesRead = await Inner.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, count)), cancellationToken);
                    Inner.Position = to;
                    await Inner.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken);
                    from += bytesRead;
                    to += bytesRead;
                    count -= bytesRead;
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        Inner.SetLength(length);
    }

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        Inner.FlushAsync(cancellationToken);
}


public abstract class AsyncStream(Stream inner) : ThrowingStream {
    protected Stream Inner { get; } = inner;

    protected override void Dispose(bool disposing) => throw new InvalidOperationException();
}

public abstract class ThrowingStream : Stream {
    #region Abstract Members

    public override bool CanRead => throw new NotImplementedException();

    public override void Flush() => throw new NotImplementedException();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    #endregion

    #region overrides

#pragma warning disable CS0162 // Unreachable code detected

    public override bool CanTimeout {
        get {
            throw new NotImplementedException();
            return base.CanTimeout;
        }
    }

    public override int ReadTimeout {
        get {
            throw new NotImplementedException();
            return base.ReadTimeout;
        }
        set {
            throw new NotImplementedException();
            base.ReadTimeout = value;
        }
    }
    public override int WriteTimeout {
        get {
            throw new NotImplementedException();
            return base.WriteTimeout;
        }
        set {
            throw new NotImplementedException();
            base.WriteTimeout = value;
        }
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) {
        throw new NotImplementedException();
        return base.BeginRead(buffer, offset, count, callback, state);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) {
        throw new NotImplementedException();
        return base.BeginWrite(buffer, offset, count, callback, state);
    }

    public override void CopyTo(Stream destination, int bufferSize) {
        throw new NotImplementedException();
        base.CopyTo(destination, bufferSize);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) {
        throw new NotImplementedException();
        return base.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override int EndRead(IAsyncResult asyncResult) {
        throw new NotImplementedException();
        return base.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult) {
        throw new NotImplementedException();
        base.EndWrite(asyncResult);
    }

    public override bool Equals(object? obj) {
        throw new NotImplementedException();
        return base.Equals(obj);
    }

    public override int GetHashCode() {
        throw new NotImplementedException();
        return base.GetHashCode();
    }

    public override int Read(Span<byte> buffer) {
        throw new NotImplementedException();
        return base.Read(buffer);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        throw new NotImplementedException();
        return base.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
        return base.ReadAsync(buffer, cancellationToken);
    }

    public override int ReadByte() {
        throw new NotImplementedException();
        return base.ReadByte();
    }

    public override string? ToString() {
        throw new NotImplementedException();
        return base.ToString();
    }

    public override void Write(ReadOnlySpan<byte> buffer) {
        throw new NotImplementedException();
        base.Write(buffer);
    }

    public override void WriteByte(byte value) {
        throw new NotImplementedException();
        base.WriteByte(value);
    }

#pragma warning restore CS0162 // Unreachable code detected

    #endregion
}
