using HRManagement.Shared.Contracts.Events;
using HRManagement.Shared.MessageBus;

namespace HRManagement.Employees.Api.Infrastructure.EventSubscription;

public class EventSubscriptionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBus _eventBus;
    private readonly ILogger<EventSubscriptionService> _logger;

    public EventSubscriptionService(
        IServiceProvider serviceProvider,
        IEventBus eventBus,
        ILogger<EventSubscriptionService> logger)
    {
        _serviceProvider = serviceProvider;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис подписок на события запускается...");

        SubscribeToCandidateHiredEvent();

        _logger.LogInformation("Сервис подписок на события успешно запущен");

        return Task.CompletedTask;
    }

    private void SubscribeToCandidateHiredEvent()
    {
        _eventBus.Subscribe<CandidateHiredEvent>(async @event =>
        {
            _logger.LogInformation(
                "Получено событие CandidateHiredEvent для кандидата {CandidateId}",
                @event.CandidateId);

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<CandidateHiredEvent>>();

            await handler.HandleAsync(@event);
        });

        _logger.LogInformation("Подписка на CandidateHiredEvent выполнена");
    }
}
