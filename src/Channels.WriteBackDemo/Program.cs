using Channels.WriteBackDemo.Channels;
using Channels.WriteBackDemo.Diagnostics;
using Channels.WriteBackDemo.Repositories;
using Channels.WriteBackDemo.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<WriteBackMetrics>();
builder.Services.AddSingleton(sp => new WriteBackQueue(
    capacity: 100,
    metrics: sp.GetRequiredService<WriteBackMetrics>()));

builder.Services.AddSingleton<ICacheWriteRepository, ConsoleCacheWriteRepository>();

// Dieselbe Instanz steht dem Demo-Producer zur Abschlusskoordination zur Verfügung.
builder.Services.AddSingleton<CacheWriteService>();
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<CacheWriteService>());
builder.Services.AddHostedService<DemoProducerService>();

await builder.Build().RunAsync();
