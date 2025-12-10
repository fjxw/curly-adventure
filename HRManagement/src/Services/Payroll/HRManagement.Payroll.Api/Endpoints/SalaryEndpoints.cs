using HRManagement.Payroll.Api.Application.DTOs;
using HRManagement.Payroll.Api.Application.Services;

namespace HRManagement.Payroll.Api.Endpoints;

public static class SalaryEndpoints
{
    public static IEndpointRouteBuilder MapSalaryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/salary")
            .WithTags("Salary")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetSalaryById")
            .WithDescription("Получить расчёт зарплаты по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/employee/{employeeId:guid}", GetByEmployee)
            .WithName("GetSalaryByEmployee")
            .WithDescription("Получить расчёты зарплаты сотрудника")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/period/{month:int}/{year:int}", GetByPeriod)
            .WithName("GetSalaryByPeriod")
            .WithDescription("Получить расчёты зарплаты за период")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/calculate", Calculate)
            .WithName("CalculateSalary")
            .WithDescription("Рассчитать зарплату")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/approve", Approve)
            .WithName("ApproveSalary")
            .WithDescription("Утвердить расчёт зарплаты")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/pay", MarkAsPaid)
            .WithName("PaySalary")
            .WithDescription("Отметить зарплату как выплаченную")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetById(
        Guid id,
        ISalaryService salaryService,
        CancellationToken cancellationToken)
    {
        var result = await salaryService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetByEmployee(
        Guid employeeId,
        ISalaryService salaryService,
        CancellationToken cancellationToken)
    {
        var result = await salaryService.GetByEmployeeAsync(employeeId, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByPeriod(
        int month,
        int year,
        ISalaryService salaryService,
        CancellationToken cancellationToken)
    {
        var result = await salaryService.GetByPeriodAsync(month, year, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> Calculate(
        CalculateSalaryRequest request,
        ISalaryService salaryService,
        CancellationToken cancellationToken)
    {
        var result = await salaryService.CalculateAsync(request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Approve(
        Guid id,
        ISalaryService salaryService,
        CancellationToken cancellationToken)
    {
        var result = await salaryService.ApproveAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> MarkAsPaid(
        Guid id,
        ISalaryService salaryService,
        CancellationToken cancellationToken)
    {
        var result = await salaryService.MarkAsPaidAsync(id, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
