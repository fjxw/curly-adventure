using HRManagement.Attendance.Api.Application.DTOs;
using HRManagement.Attendance.Api.Application.Services;

namespace HRManagement.Attendance.Api.Endpoints;

public static class WorkScheduleEndpoints
{
    public static void MapWorkScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/schedules")
            .WithTags("График работы")
            ;

        group.MapGet("/{id:guid}", async (Guid id, IWorkScheduleService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetWorkSchedule")
        .WithDescription("Получить график работы по ID");

        group.MapGet("/employee/{employeeId:guid}", async (Guid employeeId, IWorkScheduleService service) =>
        {
            var result = await service.GetByEmployeeAsync(employeeId);
            return Results.Ok(result);
        })
        .WithName("GetEmployeeSchedule")
        .WithDescription("Получить график работы сотрудника");

        group.MapPost("/", async (CreateWorkScheduleDto dto, IWorkScheduleService service) =>
        {
            var result = await service.CreateAsync(dto);
            return result.Success ? Results.Created($"/api/schedules/{result.Data?.Id}", result) : Results.BadRequest(result);
        })
        .WithName("CreateWorkSchedule")
        .WithDescription("Создать график работы");

        group.MapPost("/employee/{employeeId:guid}/default", async (Guid employeeId, IWorkScheduleService service) =>
        {
            var result = await service.CreateDefaultScheduleAsync(employeeId);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("CreateDefaultSchedule")
        .WithDescription("Создать стандартный график (Пн-Пт 9:00-18:00)");

        group.MapPut("/{id:guid}", async (Guid id, UpdateWorkScheduleDto dto, IWorkScheduleService service) =>
        {
            var result = await service.UpdateAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("UpdateWorkSchedule")
        .WithDescription("Обновить график работы");

        group.MapDelete("/{id:guid}", async (Guid id, IWorkScheduleService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("DeleteWorkSchedule")
        .WithDescription("Удалить график работы");
    }
}
