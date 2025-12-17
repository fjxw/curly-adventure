using System.Text;
using HRManagement.Recruitment.Api.Application.Services;
using HRManagement.Recruitment.Api.Endpoints;
using HRManagement.Recruitment.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Caching;
using HRManagement.Shared.MessageBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HRManagement.Recruitment.Api.Extensions;

public static class ServiceExtensions
{
    static ServiceExtensions()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public static IServiceCollection AddRecruitmentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RecruitmentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IVacancyService, VacancyService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddHttpContextAccessor();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        services.AddRabbitMqEventBus(configuration);

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Микросервис рекрутинга",
                Version = "v1",
                Description = "API для управления рекрутингом и обучением персонала"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IEndpointRouteBuilder MapRecruitmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapVacancyEndpoints();
        app.MapCandidateEndpoints();
        app.MapInterviewEndpoints();
        app.MapTrainingEndpoints();

        return app;
    }
}
