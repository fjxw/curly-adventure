using HRManagement.Payroll.Api.Application.DTOs;
using HRManagement.Payroll.Api.Application.Services;

namespace HRManagement.Payroll.Api.Endpoints;

public static class TimeSheetEndpoints
{
    public static IEndpointRouteBuilder MapTimeSheetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/timesheets")
            .WithTags("Табель рабочего времени")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetTimeSheetById")
            .WithDescription("Получить табель по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/employee/{employeeId:guid}", GetByEmployee)
            .WithName("GetTimeSheetsByEmployee")
            .WithDescription("Получить табели сотрудника")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/period/{month:int}/{year:int}", GetByPeriod)
            .WithName("GetTimeSheetsByPeriod")
            .WithDescription("Получить табели за период")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", Create)
            .WithName("CreateTimeSheet")
            .WithDescription("Создать табель")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateTimeSheet")
            .WithDescription("Обновить табель")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/approve", Approve)
            .WithName("ApproveTimeSheet")
            .WithDescription("Утвердить табель")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetById(
        Guid id,
        ITimeSheetService timeSheetService,
        CancellationToken cancellationToken)
    {
        var result = await timeSheetService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetByEmployee(
        Guid employeeId,
        ITimeSheetService timeSheetService,
        CancellationToken cancellationToken)
    {
        var result = await timeSheetService.GetByEmployeeAsync(employeeId, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByPeriod(
        int month,
        int year,
        ITimeSheetService timeSheetService,
        CancellationToken cancellationToken)
    {
        var result = await timeSheetService.GetByPeriodAsync(month, year, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateTimeSheetRequest request,
        ITimeSheetService timeSheetService,
        CancellationToken cancellationToken)
    {
        var result = await timeSheetService.CreateAsync(request, cancellationToken);
        return result.Success 
            ? Results.Created($"/api/timesheets/{result.Data?.Id}", result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateTimeSheetRequest request,
        ITimeSheetService timeSheetService,
        CancellationToken cancellationToken)
    {
        var result = await timeSheetService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Approve(
        Guid id,
        ITimeSheetService timeSheetService,
        CancellationToken cancellationToken)
    {
        var result = await timeSheetService.ApproveAsync(id, Guid.NewGuid(), cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
