namespace Channels.WriteBackDemo.Models;

public sealed record CacheWrite(
    string Key,
    string Json,
    long Version,
    DateTimeOffset EnqueuedAt);
