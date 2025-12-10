using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees")
            .WithTags("Employees")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllEmployees")
            .WithDescription("Получить всех сотрудников с пагинацией")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetEmployeeById")
            .WithDescription("Получить сотрудника по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/department/{departmentId:guid}", GetByDepartment)
            .WithName("GetEmployeesByDepartment")
            .WithDescription("Получить сотрудников отдела")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", Create)
            .WithName("CreateEmployee")
            .WithDescription("Создать сотрудника")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateEmployee")
            .WithDescription("Обновить данные сотрудника")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/terminate", Terminate)
            .WithName("TerminateEmployee")
            .WithDescription("Уволить сотрудника")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteEmployee")
            .WithDescription("Удалить сотрудника")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAll(
        int pageNumber,
        int pageSize,
        IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;
        
        var result = await employeeService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetByDepartment(
        Guid departmentId,
        IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.GetByDepartmentAsync(departmentId, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateEmployeeRequest request,
        IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.CreateAsync(request, cancellationToken);
        return result.Success 
            ? Results.Created($"/api/employees/{result.Data?.Id}", result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateEmployeeRequest request,
        IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Terminate(
        Guid id,
        TerminateEmployeeRequest request,
        IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.TerminateAsync(id, request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.DeleteAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }
}
