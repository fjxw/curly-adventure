using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class PositionHistoryEndpoints
{
    public static IEndpointRouteBuilder MapPositionHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/position-history")
            .WithTags("История должностей")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/employee/{employeeId:guid}", GetByEmployee)
            .WithName("GetPositionHistory")
            .WithDescription("Получить историю должностей сотрудника")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/current/{employeeId:guid}", GetCurrentPosition)
            .WithName("GetCurrentPosition")
            .WithDescription("Получить текущую должность сотрудника")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/change", ChangePosition)
            .WithName("ChangePosition")
            .WithDescription("Изменить должность сотрудника")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetByEmployee(Guid employeeId, IPositionHistoryService service, CancellationToken ct)
    {
        var result = await service.GetByEmployeeAsync(employeeId, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCurrentPosition(Guid employeeId, IPositionHistoryService service, CancellationToken ct)
    {
        var result = await service.GetCurrentPositionAsync(employeeId, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> ChangePosition(CreatePositionChangeRequest request, IPositionHistoryService service, CancellationToken ct)
    {
        var result = await service.ChangePositionAsync(request, ct);
        return result.Success ? Results.Created($"/api/position-history/{result.Data?.Id}", result) : Results.BadRequest(result);
    }
}
