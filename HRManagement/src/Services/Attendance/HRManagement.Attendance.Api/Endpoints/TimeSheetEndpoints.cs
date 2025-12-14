using HRManagement.Attendance.Api.Application.DTOs;
using HRManagement.Attendance.Api.Application.Services;

namespace HRManagement.Attendance.Api.Endpoints;

public static class TimeSheetEndpoints
{
    public static void MapTimeSheetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/timesheets")
            .WithTags("Табели")
            ;

        group.MapGet("/{id:guid}", async (Guid id, ITimeSheetService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetTimeSheet")
        .WithDescription("Получить табель по ID");

        group.MapGet("/employee/{employeeId:guid}", async (Guid employeeId, ITimeSheetService service) =>
        {
            var result = await service.GetByEmployeeAsync(employeeId);
            return Results.Ok(result);
        })
        .WithName("GetEmployeeTimeSheets")
        .WithDescription("Получить табели сотрудника");

        group.MapGet("/employee/{employeeId:guid}/{year:int}/{month:int}", async (
            Guid employeeId,
            int year,
            int month,
            ITimeSheetService service) =>
        {
            var result = await service.GetByEmployeeMonthAsync(employeeId, year, month);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetEmployeeTimeSheetByMonth")
        .WithDescription("Получить табель сотрудника за месяц");

        group.MapPost("/generate", async (GenerateTimeSheetDto dto, ITimeSheetService service) =>
        {
            var result = await service.GenerateAsync(dto);
            return result.Success ? Results.Created($"/api/timesheets/{result.Data?.Id}", result) : Results.BadRequest(result);
        })
        .WithName("GenerateTimeSheet")
        .WithDescription("Сформировать табель за месяц");

        group.MapPost("/{id:guid}/submit", async (Guid id, ITimeSheetService service) =>
        {
            var result = await service.SubmitAsync(id);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("SubmitTimeSheet")
        .WithDescription("Подать табель на утверждение");

        group.MapPost("/{id:guid}/approve", async (Guid id, ApproveTimeSheetDto dto, ITimeSheetService service) =>
        {
            var result = await service.ApproveAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("ApproveTimeSheet")
        .WithDescription("Утвердить табель");

        group.MapPost("/{id:guid}/reject", async (Guid id, ITimeSheetService service) =>
        {
            var result = await service.RejectAsync(id);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("RejectTimeSheet")
        .WithDescription("Отклонить табель");
    }
}
