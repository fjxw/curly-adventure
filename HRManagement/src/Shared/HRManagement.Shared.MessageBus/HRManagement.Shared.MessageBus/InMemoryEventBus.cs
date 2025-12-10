using System.Collections.Concurrent;
using System.Text.Json;

namespace HRManagement.Shared.MessageBus;

/// <summary>
/// In-memory event bus implementation for demonstration purposes.
/// In production, replace with RabbitMQ, Azure Service Bus, or similar.
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<string, List<Func<string, Task>>> _handlers = new();
    private static readonly ConcurrentQueue<EventMessage> _eventQueue = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var eventType = typeof(T).Name;
        var eventData = JsonSerializer.Serialize(@event);
        
        _eventQueue.Enqueue(new EventMessage(eventType, eventData));
        
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    await handler(eventData);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Subscribe<T>(Func<T, Task> handler) where T : class
    {
        var eventType = typeof(T).Name;
        
        var wrappedHandler = new Func<string, Task>(async (json) =>
        {
            var @event = JsonSerializer.Deserialize<T>(json);
            if (@event != null)
            {
                await handler(@event);
            }
        });

        _handlers.AddOrUpdate(
            eventType,
            _ => new List<Func<string, Task>> { wrappedHandler },
            (_, existing) =>
            {
                existing.Add(wrappedHandler);
                return existing;
            });
    }

    private record EventMessage(string EventType, string Data);
}
