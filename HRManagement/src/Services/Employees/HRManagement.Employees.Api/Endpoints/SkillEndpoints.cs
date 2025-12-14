using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class SkillEndpoints
{
    public static IEndpointRouteBuilder MapSkillEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/skills")
            .WithTags("Навыки")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAllSkills)
            .WithName("GetAllSkills")
            .WithDescription("Получить все навыки")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/", CreateSkill)
            .WithName("CreateSkill")
            .WithDescription("Создать навык")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/employee/{employeeId:guid}", GetEmployeeSkills)
            .WithName("GetEmployeeSkills")
            .WithDescription("Получить навыки сотрудника")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/by-skill/{skillId:guid}", GetEmployeesBySkill)
            .WithName("GetEmployeesBySkill")
            .WithDescription("Получить сотрудников по навыку")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/assign", AssignSkill)
            .WithName("AssignSkill")
            .WithDescription("Назначить навык сотруднику")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/employee-skill/{id:guid}", UpdateEmployeeSkill)
            .WithName("UpdateEmployeeSkill")
            .WithDescription("Обновить навык сотрудника")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/employee-skill/{id:guid}", RemoveEmployeeSkill)
            .WithName("RemoveEmployeeSkill")
            .WithDescription("Удалить навык сотрудника")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAllSkills(ISkillService service, CancellationToken ct)
    {
        var result = await service.GetAllSkillsAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateSkill(CreateSkillRequest request, ISkillService service, CancellationToken ct)
    {
        var result = await service.CreateSkillAsync(request, ct);
        return result.Success ? Results.Created($"/api/skills/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetEmployeeSkills(Guid employeeId, ISkillService service, CancellationToken ct)
    {
        var result = await service.GetEmployeeSkillsAsync(employeeId, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetEmployeesBySkill(Guid skillId, ISkillService service, CancellationToken ct)
    {
        var result = await service.GetEmployeesBySkillAsync(skillId, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> AssignSkill(AssignSkillRequest request, ISkillService service, CancellationToken ct)
    {
        var result = await service.AssignSkillAsync(request, ct);
        return result.Success ? Results.Created($"/api/skills/employee-skill/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateEmployeeSkill(Guid id, UpdateEmployeeSkillRequest request, ISkillService service, CancellationToken ct)
    {
        var result = await service.UpdateEmployeeSkillAsync(id, request, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RemoveEmployeeSkill(Guid id, ISkillService service, CancellationToken ct)
    {
        var result = await service.RemoveEmployeeSkillAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }
}
