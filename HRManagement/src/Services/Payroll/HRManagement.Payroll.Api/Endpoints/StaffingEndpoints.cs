using HRManagement.Payroll.Api.Application.DTOs;
using HRManagement.Payroll.Api.Application.Services;

namespace HRManagement.Payroll.Api.Endpoints;

public static class StaffingEndpoints
{
    public static IEndpointRouteBuilder MapStaffingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/staffing")
            .WithTags("Штатное расписание")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllStaffingTables")
            .WithDescription("Получить все штатные расписания")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/active", GetActive)
            .WithName("GetActiveStaffingTable")
            .WithDescription("Получить активное штатное расписание")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetStaffingTableById")
            .WithDescription("Получить штатное расписание по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateStaffingTable")
            .WithDescription("Создать штатное расписание")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateStaffingTable")
            .WithDescription("Обновить штатное расписание")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/positions", AddPosition)
            .WithName("AddStaffingPosition")
            .WithDescription("Добавить штатную единицу")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}/positions", GetPositions)
            .WithName("GetStaffingPositions")
            .WithDescription("Получить штатные единицы")
            .Produces(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> GetAll(
        IStaffingService staffingService,
        CancellationToken cancellationToken)
    {
        var result = await staffingService.GetAllAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetActive(
        IStaffingService staffingService,
        CancellationToken cancellationToken)
    {
        var result = await staffingService.GetActiveAsync(cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IStaffingService staffingService,
        CancellationToken cancellationToken)
    {
        var result = await staffingService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Create(
        CreateStaffingTableRequest request,
        IStaffingService staffingService,
        CancellationToken cancellationToken)
    {
        var result = await staffingService.CreateAsync(request, cancellationToken);
        return result.Success 
            ? Results.Created($"/api/staffing/{result.Data?.Id}", result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateStaffingTableRequest request,
        IStaffingService staffingService,
        CancellationToken cancellationToken)
    {
        var result = await staffingService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> AddPosition(
        Guid id,
        CreateStaffingPositionRequest request,
        IStaffingService staffingService,
        CancellationToken cancellationToken)
    {
        var result = await staffingService.AddPositionAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetPositions(
        Guid id,
        IStaffingService staffingService,
        CancellationToken cancellationToken)
    {
        var result = await staffingService.GetPositionsAsync(id, cancellationToken);
        return Results.Ok(result);
    }
}
