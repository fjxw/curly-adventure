using HRManagement.Documents.Api.Application.Services;
using HRManagement.Documents.Api.Infrastructure.Data;
using HRManagement.Shared.MessageBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace HRManagement.Documents.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DocumentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentTemplateService, DocumentTemplateService>();

        services.AddRabbitMqEventBus(configuration);

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HR Management - Документы API",
                Version = "v1",
                Description = "API для управления HR-документами, шаблонами и электронным подписанием"
            });
        });

        return services;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
