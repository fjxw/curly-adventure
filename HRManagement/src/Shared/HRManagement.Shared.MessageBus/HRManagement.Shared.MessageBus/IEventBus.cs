namespace HRManagement.Shared.MessageBus;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
    void Subscribe<T>(Func<T, Task> handler) where T : class;
}
