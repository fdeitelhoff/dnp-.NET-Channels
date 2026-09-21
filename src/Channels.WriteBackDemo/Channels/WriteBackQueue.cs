using System.Diagnostics;
using System.Threading.Channels;
using Channels.WriteBackDemo.Diagnostics;
using Channels.WriteBackDemo.Models;

namespace Channels.WriteBackDemo.Channels;

public sealed class WriteBackQueue
{
    private readonly Channel<CacheWrite> _channel;
    private readonly WriteBackMetrics _metrics;

    public WriteBackQueue(int capacity, WriteBackMetrics metrics)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _metrics = metrics;
        _channel = Channel.CreateBounded<CacheWrite>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
    }

    public ChannelReader<CacheWrite> Reader => _channel.Reader;

    public async ValueTask EnqueueAsync(
        CacheWrite write,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(write);

        _metrics.EnqueueStarted();
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            await _channel.Writer.WriteAsync(write, cancellationToken);
            _metrics.Accepted(
                Stopwatch.GetElapsedTime(startedAt).TotalSeconds);
        }
        catch
        {
            _metrics.Rejected();
            throw;
        }
    }

    public bool TryComplete(Exception? error = null) =>
        _channel.Writer.TryComplete(error);
}
