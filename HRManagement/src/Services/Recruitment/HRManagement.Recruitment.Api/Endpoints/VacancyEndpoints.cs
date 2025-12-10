using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Application.Services;

namespace HRManagement.Recruitment.Api.Endpoints;

public static class VacancyEndpoints
{
    public static IEndpointRouteBuilder MapVacancyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vacancies")
            .WithTags("Vacancies")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllVacancies")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/open", GetOpen)
            .WithName("GetOpenVacancies")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetVacancyById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateVacancy")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateVacancy")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteVacancy")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAll(IVacancyService service, CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetOpen(IVacancyService service, CancellationToken ct)
    {
        var result = await service.GetOpenAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(Guid id, IVacancyService service, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Create(CreateVacancyRequest request, IVacancyService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.Success ? Results.Created($"/api/vacancies/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(Guid id, UpdateVacancyRequest request, IVacancyService service, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Delete(Guid id, IVacancyService service, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }
}
