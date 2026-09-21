using Channels.WriteBackDemo.Models;

namespace Channels.WriteBackDemo.Repositories;

public interface ICacheWriteRepository
{
    Task UpsertBatchAsync(
        IReadOnlyCollection<CacheWrite> batch,
        CancellationToken cancellationToken);
}
