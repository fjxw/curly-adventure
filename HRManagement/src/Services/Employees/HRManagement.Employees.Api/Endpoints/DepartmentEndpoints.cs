using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class DepartmentEndpoints
{
    public static IEndpointRouteBuilder MapDepartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/departments")
            .WithTags("Отделы")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllDepartments")
            .WithDescription("Получить все отделы")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/top-level", GetTopLevel)
            .WithName("GetTopLevelDepartments")
            .WithDescription("Получить отделы верхнего уровня")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetDepartmentById")
            .WithDescription("Получить отдел по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateDepartment")
            .WithDescription("Создать отдел")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateDepartment")
            .WithDescription("Обновить отдел")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteDepartment")
            .WithDescription("Удалить отдел")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAll(
        IDepartmentService departmentService,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.GetAllAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetTopLevel(
        IDepartmentService departmentService,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.GetTopLevelAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IDepartmentService departmentService,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Create(
        CreateDepartmentRequest request,
        IDepartmentService departmentService,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.CreateAsync(request, cancellationToken);
        return result.Success 
            ? Results.Created($"/api/departments/{result.Data?.Id}", result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateDepartmentRequest request,
        IDepartmentService departmentService,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IDepartmentService departmentService,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.DeleteAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
