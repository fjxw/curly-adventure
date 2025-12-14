namespace HRManagement.Shared.MessageBus;

/// <summary>
/// Interface for event handlers that process messages from the event bus.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
