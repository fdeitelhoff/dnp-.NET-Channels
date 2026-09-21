using System.Collections.Concurrent;
using Channels.WriteBackDemo.Models;

namespace Channels.WriteBackDemo.Repositories;

public sealed class ConsoleCacheWriteRepository(
    ILogger<ConsoleCacheWriteRepository> logger) : ICacheWriteRepository
{
    private readonly ConcurrentDictionary<string, CacheWrite> _cache = new();

    public async Task UpsertBatchAsync(
        IReadOnlyCollection<CacheWrite> batch,
        CancellationToken cancellationToken)
    {
        // Simuliert einen langsameren I/O-Zugriff, damit Batching sichtbar bleibt.
        await Task.Delay(TimeSpan.FromMilliseconds(40), cancellationToken);

        foreach (var item in batch)
        {
            _cache.AddOrUpdate(
                item.Key,
                item,
                (_, current) => item.Version > current.Version
                    ? item
                    : current);
        }

        logger.LogInformation(
            "Batch mit {BatchSize} Einträgen verarbeitet; {KeyCount} Schlüssel gespeichert.",
            batch.Count,
            _cache.Count);
    }
}
