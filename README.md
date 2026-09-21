# High-Performance-Pipelines mit C# Channels

Diese Projektmappe enthält die drei Listings des Fachartikels als ausführbare .NET-8-Anwendung für Visual Studio 2022. Die Beispielanwendung erzeugt 250 Cache-Änderungen, schreibt sie mit Backpressure in einen bounded Channel und persistiert sie in begrenzten Batches in einem In-Memory-Repository.

## Voraussetzungen

- Visual Studio 2022 ab Version 17.8
- Workload **.NET-Desktopentwicklung** oder **ASP.NET und Webentwicklung**
- .NET 8 SDK

## Start

1. `ChannelsArticleExamples.sln` in Visual Studio 2022 öffnen.
2. `Channels.WriteBackDemo` als Startprojekt auswählen.
3. Mit `F5` oder `Strg+F5` starten.

Die Anwendung beendet sich, nachdem der Producer den Writer abgeschlossen und der Consumer alle Batches verarbeitet hat.

## Zuordnung zum Artikel

| Artikel | Datei | Zweck |
| --- | --- | --- |
| Listing 1 | `Channels/WriteBackQueue.cs` | Bounded Channel mit `Wait`, einem Reader und mehreren Writern |
| Listing 2 | `Services/CacheWriteService.cs` | Consumer-Schleife mit `WaitToReadAsync`, `TryRead` und begrenzten Batches |
| Listing 3 | `Diagnostics/WriteBackMetrics.cs` | Counter, Histogramm und Observable Gauge |

Die übrigen Dateien machen die Listings direkt ausführbar:

- `Services/DemoProducerService.cs` simuliert mehrere Schreibvorgänge und koordiniert den Abschluss.
- `Repositories/ConsoleCacheWriteRepository.cs` bildet ein idempotentes, versionsbasiertes Upsert nach.
- `Program.cs` registriert Queue, Metriken, Repository und Dienste im DI-Container.

## Bewusst gewählte Grenzen

Der Channel lebt ausschließlich im Prozessspeicher. Die Demo bildet daher weder dauerhafte Zustellung noch Exactly-once-Semantik ab. Für fachlich unverlierbare Aufträge braucht die Anwendung zusätzlich etwa eine Outbox, eine Datenbanktabelle oder einen Broker.

Die Projektmappe zielt auf .NET 8, damit sie mit Visual Studio 2022 breit nutzbar bleibt. Der im Artikel erwähnte Rendezvous-Channel mit `Channel.CreateBounded<T>(0)` setzt .NET 10 voraus und gehört deshalb nicht zum ausführbaren Beispiel.
