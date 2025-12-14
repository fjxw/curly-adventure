using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRManagement.Shared.MessageBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }

    public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        return services;
    }

    public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection services, Action<RabbitMqSettings> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        return services;
    }
}
