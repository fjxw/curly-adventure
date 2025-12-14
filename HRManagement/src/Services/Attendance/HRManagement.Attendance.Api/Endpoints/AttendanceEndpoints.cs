using HRManagement.Attendance.Api.Application.DTOs;
using HRManagement.Attendance.Api.Application.Services;

namespace HRManagement.Attendance.Api.Endpoints;

public static class AttendanceEndpoints
{
    public static void MapAttendanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/attendance")
            .WithTags("Посещаемость")
            ;

        group.MapGet("/{id:guid}", async (Guid id, IAttendanceService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetAttendanceRecord")
        .WithDescription("Получить запись посещаемости по ID");

        group.MapGet("/employee/{employeeId:guid}", async (
            Guid employeeId,
            DateTime? startDate,
            DateTime? endDate,
            IAttendanceService service) =>
        {
            var result = await service.GetByEmployeeAsync(employeeId, startDate, endDate);
            return Results.Ok(result);
        })
        .WithName("GetEmployeeAttendance")
        .WithDescription("Получить записи посещаемости сотрудника");

        group.MapGet("/date/{date:datetime}", async (DateTime date, IAttendanceService service) =>
        {
            var result = await service.GetByDateAsync(date);
            return Results.Ok(result);
        })
        .WithName("GetAttendanceByDate")
        .WithDescription("Получить все записи посещаемости за дату");

        group.MapPost("/", async (CreateAttendanceRecordDto dto, IAttendanceService service) =>
        {
            var result = await service.CreateAsync(dto);
            return result.Success ? Results.Created($"/api/attendance/{result.Data?.Id}", result) : Results.BadRequest(result);
        })
        .WithName("CreateAttendanceRecord")
        .WithDescription("Создать запись посещаемости");

        group.MapPut("/{id:guid}", async (Guid id, UpdateAttendanceRecordDto dto, IAttendanceService service) =>
        {
            var result = await service.UpdateAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("UpdateAttendanceRecord")
        .WithDescription("Обновить запись посещаемости");

        group.MapDelete("/{id:guid}", async (Guid id, IAttendanceService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("DeleteAttendanceRecord")
        .WithDescription("Удалить запись посещаемости");

        group.MapPost("/checkin", async (CheckInDto dto, IAttendanceService service) =>
        {
            var result = await service.CheckInAsync(dto);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("CheckIn")
        .WithDescription("Отметка прихода на работу");

        group.MapPost("/checkout", async (CheckOutDto dto, IAttendanceService service) =>
        {
            var result = await service.CheckOutAsync(dto);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("CheckOut")
        .WithDescription("Отметка ухода с работы");
    }
}
