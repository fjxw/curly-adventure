using Microsoft.Extensions.DependencyInjection;

namespace HRManagement.Shared.MessageBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }
}
