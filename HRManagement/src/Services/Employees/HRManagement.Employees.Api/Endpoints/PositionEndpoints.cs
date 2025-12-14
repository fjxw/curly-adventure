using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class PositionEndpoints
{
    public static IEndpointRouteBuilder MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/positions")
            .WithTags("Должности")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllPositions")
            .WithDescription("Получить все должности")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetPositionById")
            .WithDescription("Получить должность по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/department/{departmentId:guid}", GetByDepartment)
            .WithName("GetPositionsByDepartment")
            .WithDescription("Получить должности отдела")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", Create)
            .WithName("CreatePosition")
            .WithDescription("Создать должность")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdatePosition")
            .WithDescription("Обновить должность")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeletePosition")
            .WithDescription("Удалить должность")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/responsibilities", AddResponsibility)
            .WithName("AddResponsibility")
            .WithDescription("Добавить должностную обязанность")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAll(
        IPositionService positionService,
        CancellationToken cancellationToken)
    {
        var result = await positionService.GetAllAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IPositionService positionService,
        CancellationToken cancellationToken)
    {
        var result = await positionService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetByDepartment(
        Guid departmentId,
        IPositionService positionService,
        CancellationToken cancellationToken)
    {
        var result = await positionService.GetByDepartmentAsync(departmentId, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreatePositionRequest request,
        IPositionService positionService,
        CancellationToken cancellationToken)
    {
        var result = await positionService.CreateAsync(request, cancellationToken);
        return result.Success 
            ? Results.Created($"/api/positions/{result.Data?.Id}", result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdatePositionRequest request,
        IPositionService positionService,
        CancellationToken cancellationToken)
    {
        var result = await positionService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IPositionService positionService,
        CancellationToken cancellationToken)
    {
        var result = await positionService.DeleteAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> AddResponsibility(
        Guid id,
        CreateJobResponsibilityRequest request,
        IPositionService positionService,
        CancellationToken cancellationToken)
    {
        var result = await positionService.AddResponsibilityAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }
}
