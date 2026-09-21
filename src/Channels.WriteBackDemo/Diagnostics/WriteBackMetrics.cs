using System.Diagnostics.Metrics;

namespace Channels.WriteBackDemo.Diagnostics;

public sealed class WriteBackMetrics
{
    private readonly Counter<long> _accepted;
    private readonly Counter<long> _processed;
    private readonly Counter<long> _failed;
    private readonly Histogram<double> _enqueueWait;
    private long _outstanding;

    public WriteBackMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Example.WriteBack");

        _accepted = meter.CreateCounter<long>("writeback.accepted");
        _processed = meter.CreateCounter<long>("writeback.processed");
        _failed = meter.CreateCounter<long>("writeback.failed");
        _enqueueWait = meter.CreateHistogram<double>(
            "writeback.enqueue_wait",
            unit: "s");

        meter.CreateObservableGauge(
            "writeback.outstanding",
            () => Interlocked.Read(ref _outstanding));
    }

    // Vor WriteAsync aufrufen; umfasst Warten, Puffer und Verarbeitung.
    public void EnqueueStarted() =>
        Interlocked.Increment(ref _outstanding);

    // Nach erfolgreichem WriteAsync aufrufen.
    public void Accepted(double waitSeconds)
    {
        _accepted.Add(1);
        _enqueueWait.Record(waitSeconds);
    }

    // Bei Abbruch oder geschlossenem Channel aufrufen.
    public void Rejected() =>
        Interlocked.Decrement(ref _outstanding);

    // Nach erfolgreichem Persistieren aufrufen.
    public void Processed(int count)
    {
        Interlocked.Add(ref _outstanding, -count);
        _processed.Add(count);
    }

    public void FailedPermanently(int count)
    {
        Interlocked.Add(ref _outstanding, -count);
        _failed.Add(count);
    }
}
