using System.Text.Json;
using Channels.WriteBackDemo.Channels;
using Channels.WriteBackDemo.Models;

namespace Channels.WriteBackDemo.Services;

public sealed class DemoProducerService(
    WriteBackQueue queue,
    CacheWriteService consumer,
    IHostApplicationLifetime applicationLifetime,
    ILogger<DemoProducerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            for (var version = 1; version <= 250; version++)
            {
                var key = $"product:{version % 25:D2}";
                var json = JsonSerializer.Serialize(new
                {
                    Key = key,
                    Version = version,
                    UpdatedAt = DateTimeOffset.UtcNow
                });

                await queue.EnqueueAsync(
                    new CacheWrite(
                        key,
                        json,
                        version,
                        DateTimeOffset.UtcNow),
                    stoppingToken);
            }

            logger.LogInformation(
                "Producer hat alle Elemente eingestellt und schließt den Writer.");
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Demo-Producer stopped.");
        }
        finally
        {
            queue.TryComplete();
        }

        try
        {
            // Reader.Completion allein garantiert nicht, dass das letzte
            // bereits entnommene Batch vollständig persistiert ist.
            await consumer.ProcessingCompleted.WaitAsync(
                TimeSpan.FromSeconds(30),
                CancellationToken.None);

            logger.LogInformation(
                "Alle angenommenen Elemente sind verarbeitet.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Die Verarbeitung endete nicht kontrolliert.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }
}
