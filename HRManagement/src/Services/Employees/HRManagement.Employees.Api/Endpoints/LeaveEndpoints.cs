using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class LeaveEndpoints
{
    public static IEndpointRouteBuilder MapLeaveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leaves")
            .WithTags("Отпуска")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetLeaveById")
            .WithDescription("Получить заявку на отпуск по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/employee/{employeeId:guid}", GetByEmployee)
            .WithName("GetLeavesByEmployee")
            .WithDescription("Получить отпуска сотрудника")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/pending", GetPending)
            .WithName("GetPendingLeaves")
            .WithDescription("Получить заявки на рассмотрении")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/remaining/{employeeId:guid}/{year:int}", GetRemainingDays)
            .WithName("GetRemainingLeaveDays")
            .WithDescription("Получить остаток дней отпуска")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", Create)
            .WithName("CreateLeave")
            .WithDescription("Создать заявку на отпуск")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateLeave")
            .WithDescription("Обновить заявку на отпуск")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/approve", Approve)
            .WithName("ApproveLeave")
            .WithDescription("Одобрить/отклонить заявку")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/cancel", Cancel)
            .WithName("CancelLeave")
            .WithDescription("Отменить заявку")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetById(Guid id, ILeaveService service, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetByEmployee(Guid employeeId, ILeaveService service, CancellationToken ct)
    {
        var result = await service.GetByEmployeeAsync(employeeId, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPending(ILeaveService service, CancellationToken ct)
    {
        var result = await service.GetPendingAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRemainingDays(Guid employeeId, int year, ILeaveService service, CancellationToken ct)
    {
        var result = await service.GetRemainingDaysAsync(employeeId, year, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(CreateLeaveRequest request, ILeaveService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.Success ? Results.Created($"/api/leaves/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(Guid id, UpdateLeaveRequest request, ILeaveService service, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Approve(Guid id, ApproveLeaveRequest request, ILeaveService service, CancellationToken ct)
    {
        var result = await service.ApproveAsync(id, Guid.NewGuid(), request, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Cancel(Guid id, ILeaveService service, CancellationToken ct)
    {
        var result = await service.CancelAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
