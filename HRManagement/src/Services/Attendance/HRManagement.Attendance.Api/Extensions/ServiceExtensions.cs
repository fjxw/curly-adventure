using HRManagement.Attendance.Api.Application.Services;
using HRManagement.Attendance.Api.Infrastructure.Data;
using HRManagement.Shared.MessageBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace HRManagement.Attendance.Api.Extensions;

public static class ServiceExtensions
{
    static ServiceExtensions()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IWorkScheduleService, WorkScheduleService>();
        services.AddScoped<ITimeSheetService, TimeSheetService>();

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
                Title = "Микросервис посещаемости",
                Version = "v1",
                Description = "API для управления посещаемостью, графиками работы и табелями учёта рабочего времени"
            });
        });

        return services;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
