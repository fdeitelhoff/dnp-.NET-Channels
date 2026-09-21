using Channels.WriteBackDemo.Channels;
using Channels.WriteBackDemo.Diagnostics;
using Channels.WriteBackDemo.Models;
using Channels.WriteBackDemo.Repositories;

namespace Channels.WriteBackDemo.Services;

public sealed class CacheWriteService(
    WriteBackQueue queue,
    ICacheWriteRepository repository,
    WriteBackMetrics metrics,
    ILogger<CacheWriteService> logger) : BackgroundService
{
    private readonly TaskCompletionSource _processingCompleted =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task ProcessingCompleted => _processingCompleted.Task;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var batch = new List<CacheWrite>(capacity: 100);

        try
        {
            while (await queue.Reader.WaitToReadAsync(stoppingToken))
            {
                while (batch.Count < 100 &&
                       queue.Reader.TryRead(out var item))
                {
                    batch.Add(item);
                }

                try
                {
                    await repository.UpsertBatchAsync(
                        batch,
                        stoppingToken);

                    metrics.Processed(batch.Count);
                }
                catch
                {
                    // Die Demo führt keine Retries aus. Produktiv gehört hier
                    // eine fachlich passende Retry- oder Dead-Letter-Policy hin.
                    metrics.FailedPermanently(batch.Count);
                    throw;
                }
                finally
                {
                    batch.Clear();
                }
            }

            _processingCompleted.TrySetResult();
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Write-back consumer stopped.");
            _processingCompleted.TrySetCanceled(stoppingToken);
        }
        catch (Exception exception)
        {
            logger.LogCritical(
                exception,
                "Write-back consumer failed.");
            _processingCompleted.TrySetException(exception);
            throw;
        }
    }
}
